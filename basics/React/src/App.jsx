import React, { useState, useEffect, useCallback, useRef } from 'react';
import { v4 as uuidv4 } from 'uuid';

// --- Global Constants ---
// API Endpointleri (Controller'ınızdaki route'lar ile birebir aynı)
const SAVE_PLAN_ENDPOINT = '/api/admin/save-seating-plan';
const GET_PLAN_ENDPOINT = '/api/admin/get-seating-plan';
const LIST_PLANS_ENDPOINT = '/api/admin/list-seating-plans';

// Koltukların temel boyutları
const SEAT_SIZE = 40;
const SEAT_MARGIN = 8;
const PLAN_OFFSET_X = 280;
const PLAN_OFFSET_Y = 100;

// Varsayılan boş bir plan yapısı
const initialPlan = {
  salonAdi: "Yeni Salon",
  koltuklar: [], // { id, blok, sira, numara, x, y, status }
  bloklar: [],   // { blokAdi, startX, startY, rows, capacityPerRow, startNumber }
  sahne: { width: 600, height: 35, x: 50, y: 30 }
};

// --- Helper Functions ---

const getSymmetricSeatNumber = (c, capacity) => {
  const single_capacity = Math.ceil(capacity / 2);
  if (c < single_capacity) {
    return 2 * c + 1;
  } else {
    const cift_capacity = Math.floor(capacity / 2);
    const relative_index = c - single_capacity;
    const numara = (cift_capacity - 1 - relative_index) * 2 + 2;
    return numara;
  }
}

const getReverseSymmetricSeatNumber = (c, capacity) => {
  const cift_capacity = Math.floor(capacity / 2);
  const tek_capacity = Math.ceil(capacity / 2);

  if (c < cift_capacity) {
    return 2 * (c + 1);
  } else {
    const relative_index = c - cift_capacity;
    const numara = (tek_capacity - 1 - relative_index) * 2 + 1;
    return numara;
  }
}

const downloadJson = (data) => {
  const json = JSON.stringify(data, null, 2);
  const blob = new Blob([json], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `${data.salonAdi.replace(/\s/g, '_')}_Plan.json`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
};

// --- React Component ---

const App = () => {
  // Not: Firebase state'leri ve useEffect'leri tamamen kaldırıldı.
  // Artık isDbReady kontrolü yok, doğrudan API'ye istek atacağız.

  const [isSaving, setIsSaving] = useState(false);
  const [notification, setNotification] = useState('');

  const [planData, setPlanData] = useState(initialPlan);
  const canvasRef = useRef(null);
  const [scale, setScale] = useState(1);
  const [planHistory, setPlanHistory] = useState([initialPlan]);
  const [historyIndex, setHistoryIndex] = useState(0);
  const [savedPlans, setSavedPlans] = useState([]);
  const [activePlanId, setActivePlanId] = useState(initialPlan.salonAdi);

  // Form state
  const [blockName, setBlockName] = useState('A');
  const [rowCapacity, setRowCapacity] = useState(10);
  const [numRows, setNumRows] = useState(5);
  const [startNumber, setStartNumber] = useState(1);
  const [startLetter, setStartLetter] = useState('A');
  const [direction, setDirection] = useState('symmetric');

  // Interactive state
  const [mode, setMode] = useState('block_add');
  const [pendingScroll, setPendingScroll] = useState({ scrollTop: 0, scrollLeft: 0, apply: false });
  const planAreaRef = useRef(null);

  // --- API Veri Çekme (ASP.NET Core API'den Kayıtlı Salonları Çekme) ---

  const fetchSavedPlans = useCallback(async () => {
    try {
      // GET: /api/admin/list-seating-plans
      const response = await fetch(LIST_PLANS_ENDPOINT);

      if (!response.ok) {
        // Eğer backend henüz çalışmıyorsa veya hata döndürüyorsa sessizce logla
        console.warn(`API listesi çekilemedi: ${response.statusText}`);
        return;
      }

      const data = await response.json();

      // Controller'dan gelen liste formatına göre işle
      if (Array.isArray(data)) {
        setSavedPlans(data.map(plan => ({
          id: plan.salonAdi,
          salonAdi: plan.salonAdi,
          planAdi: plan.planAdi,
          koltuklar: [] // Liste görünümü için koltuk verisine gerek yok
        })));
        console.log("Kayıtlı salonlar API'den güncellendi.");
      }

    } catch (error) {
      console.error("Kayıtlı planlar çekilirken hata:", error);
      setNotification("Hata: Sunucuya bağlanılamadı. Backend'in çalıştığından emin olun.");
    }
  }, []);

  // Komponent yüklendiğinde salon listesini çek
  useEffect(() => {
    fetchSavedPlans();
  }, [fetchSavedPlans]);


  // --- Keyboard Shortcuts ---
  useEffect(() => {
    const handleKeyDown = (e) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'z') {
        e.preventDefault();
        handleUndo();
      }
      if ((e.ctrlKey || e.metaKey) && (e.key === 'y' || (e.shiftKey && e.key === 'z'))) {
        e.preventDefault();
        handleRedo();
      }
      if ((e.ctrlKey || e.metaKey) && e.key === 'a') {
        e.preventDefault();
        handleSelectAll();
      }
      if ((e.ctrlKey || e.metaKey) && e.key === 'd') {
        e.preventDefault();
        handleDeselectAll();
      }

      const hasSelectedSeats = planData.koltuklar.some(s => s.isSelected);
      if (hasSelectedSeats) {
        if (e.key === 'ArrowUp') { e.preventDefault(); handleMoveSelected(0, -5); }
        if (e.key === 'ArrowDown') { e.preventDefault(); handleMoveSelected(0, 5); }
        if (e.key === 'ArrowLeft') { e.preventDefault(); handleMoveSelected(-5, 0); }
        if (e.key === 'ArrowRight') { e.preventDefault(); handleMoveSelected(5, 0); }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [historyIndex, planHistory, planData]);

  // --- Scroll Logic ---
  useEffect(() => {
    if (pendingScroll.apply) {
      const area = planAreaRef.current;
      if (area) {
        area.scrollTop += pendingScroll.scrollTop;
        area.scrollLeft += pendingScroll.scrollLeft;
      }
      setPendingScroll({ scrollTop: 0, scrollLeft: 0, apply: false });
    }
  }, [pendingScroll]);

  // --- Utility Functions ---

  const indexToLetter = (index) => {
    return String.fromCharCode(65 + index);
  };

  const letterToIndex = (letter) => {
    return letter.toUpperCase().charCodeAt(0) - 65;
  };

  const ensureCanvasFits = (plan) => {
    const padding = 20;
    let minX = Infinity, minY = Infinity;

    plan.koltuklar.forEach(s => {
      minX = Math.min(minX, s.x || 0);
      minY = Math.min(minY, s.y || 0);
    });

    plan.bloklar.forEach(b => {
      minX = Math.min(minX, b.startX || 0);
      minY = Math.min(minY, b.startY || 0);
    });

    if (!isFinite(minX)) minX = 0;
    if (!isFinite(minY)) minY = 0;

    const shiftX = minX < padding ? (padding - minX) : 0;
    const shiftY = minY < padding ? (padding - minY) : 0;

    if (shiftX === 0 && shiftY === 0) return { plan, shiftX: 0, shiftY: 0 };

    const normalized = {
      ...plan,
      koltuklar: plan.koltuklar.map(s => ({ ...s, x: s.x + shiftX, y: s.y + shiftY })),
      bloklar: plan.bloklar.map(b => ({ ...b, startX: b.startX + shiftX, startY: b.startY + shiftY })),
      sahne: { ...plan.sahne }
    };

    return { plan: normalized, shiftX, shiftY };
  };

  const updateHistory = (newPlanData) => {
    const newPlanHistory = [...planHistory.slice(0, historyIndex + 1), newPlanData];
    setPlanHistory(newPlanHistory);
    setHistoryIndex(newPlanHistory.length - 1);
  };

  const addBlock = (x, y) => {
    const newKoltuklar = [];
    const existingSeats = planData.koltuklar;
    const seatCapacity = parseInt(rowCapacity);
    const numRowsInt = parseInt(numRows);
    const startNum = parseInt(startNumber);
    const startLtrIndex = letterToIndex(startLetter);

    if (seatCapacity <= 0 || numRowsInt <= 0) {
      setNotification("Satır kapasitesi ve sayısı 1'den büyük olmalıdır.");
      return;
    }

    const newBlockData = {
      blokAdi: blockName,
      startX: x,
      startY: y,
      rows: numRowsInt,
      capacityPerRow: seatCapacity,
      startNumber: startNum,
      startLetter: startLetter,
      direction: direction,
    };

    for (let r = 0; r < numRowsInt; r++) {
      const siraHarfi = indexToLetter(startLtrIndex + r);
      for (let c = 0; c < seatCapacity; c++) {
        let seatX, seatY, seatNum;

        seatY = y + (r * (SEAT_SIZE + SEAT_MARGIN));

        switch (direction) {
          case 'left':
            seatX = x + (c * (SEAT_SIZE + SEAT_MARGIN));
            seatNum = startNum + c;
            break;
          case 'right':
            seatX = x + (c * (SEAT_SIZE + SEAT_MARGIN));
            seatNum = startNum + (seatCapacity - 1 - c);
            break;
          case 'symmetric':
            seatX = x + (c * (SEAT_SIZE + SEAT_MARGIN));
            seatNum = getSymmetricSeatNumber(c, seatCapacity);
            break;
          case 'reverse_symmetric':
            seatX = x + (c * (SEAT_SIZE + SEAT_MARGIN));
            seatNum = getReverseSymmetricSeatNumber(c, seatCapacity);
            break;
          default:
            seatX = x + (c * (SEAT_SIZE + SEAT_MARGIN));
            seatNum = startNum + c;
        }

        newKoltuklar.push({
          id: uuidv4(),
          blok: blockName,
          sira: siraHarfi,
          numara: seatNum,
          x: seatX,
          y: seatY,
          status: 'available'
        });
      }
    }

    const newPlanData = {
      ...planData,
      koltuklar: [...existingSeats, ...newKoltuklar],
      bloklar: [...planData.bloklar, newBlockData]
    };

    const { plan: normalized, shiftX, shiftY } = ensureCanvasFits(newPlanData);

    setPlanData(normalized);
    updateHistory(normalized);

    if ((shiftX && shiftX > 0) || (shiftY && shiftY > 0)) {
      setPendingScroll({ scrollTop: shiftY, scrollLeft: shiftX, apply: true });
    }

    setMode('move_seat');
    showNotification(`Blok ${blockName} eklendi. Yön: ${direction}`);
  };

  // --- Event Handlers ---

  const handleSeatClick = (seatId) => {
    if (mode === 'move_seat') {
      setPlanData(prev => ({
        ...prev,
        koltuklar: prev.koltuklar.map(seat =>
          seat.id === seatId ? { ...seat, isSelected: !seat.isSelected } : seat
        )
      }));
    } else {
      showNotification("Koltukları taşımak için 'Koltuk Taşı' modunu seçin.");
    }
  };

  const handleBlockAdd = () => {
    if (!planData.salonAdi || planData.salonAdi.trim() === initialPlan.salonAdi) {
      setNotification("Lütfen önce Salona anlamlı bir isim verin.");
      return;
    }
    setMode('block_add');
    showNotification("Blok ekleme modundasınız. Plan alanına tıklayarak bloğu yerleştirin.");
  };

  const handleUndo = () => {
    if (historyIndex > 0) {
      const newIndex = historyIndex - 1;
      setHistoryIndex(newIndex);
      setPlanData(planHistory[newIndex]);
      showNotification("Geri alındı.");
    } else {
      showNotification("Geri alacak işlem yok.");
    }
  };

  const handleRedo = () => {
    if (historyIndex < planHistory.length - 1) {
      const newIndex = historyIndex + 1;
      setHistoryIndex(newIndex);
      setPlanData(planHistory[newIndex]);
      showNotification("İleri alındı.");
    } else {
      showNotification("İleri alacak işlem yok.");
    }
  };

  const handleSelectAll = () => {
    const newPlanData = {
      ...planData,
      koltuklar: planData.koltuklar.map(seat => ({ ...seat, isSelected: true }))
    };
    setPlanData(newPlanData);
    showNotification("Tüm koltuklar seçildi.");
  };

  const handleDeselectAll = () => {
    const newPlanData = {
      ...planData,
      koltuklar: planData.koltuklar.map(seat => ({ ...seat, isSelected: false }))
    };
    setPlanData(newPlanData);
    showNotification("Tüm seçimler kaldırıldı.");
  };

  const handleSelectRow = (blockName, rowLetter) => {
    const rowSeats = planData.koltuklar.filter(s => s.blok === blockName && s.sira === rowLetter);
    const allCurrentlySelected = rowSeats.length > 0 && rowSeats.every(s => s.isSelected);
    const shouldSelect = !allCurrentlySelected; // Toggle

    const updatedPlanData = {
      ...planData,
      koltuklar: planData.koltuklar.map(seat =>
        seat.blok === blockName && seat.sira === rowLetter ? { ...seat, isSelected: shouldSelect } : seat
      )
    };

    setPlanData(updatedPlanData);
    showNotification(shouldSelect ? `${blockName}${rowLetter} satırı seçildi.` : `${blockName}${rowLetter} satırı bırakıldı.`);
  };

  const handleDeleteSelected = () => {
    const selectedCount = planData.koltuklar.filter(s => s.isSelected).length;
    if (selectedCount === 0) {
      showNotification("Silmek için koltuk seçin.");
      return;
    }

    const updatedKoltuklar = planData.koltuklar.filter(seat => !seat.isSelected);
    const usedBlockNames = new Set(updatedKoltuklar.map(seat => seat.blok));
    const updatedBloklar = planData.bloklar.filter(blok => usedBlockNames.has(blok.blokAdi));

    const updatedPlanData = {
      ...planData,
      koltuklar: updatedKoltuklar,
      bloklar: updatedBloklar
    };

    setPlanData(updatedPlanData);
    updateHistory(updatedPlanData);
    showNotification(`${selectedCount} koltuk silindi.`);
  };

  const handlePlanAreaClick = (e) => {
    if (mode === 'block_add') {
      const area = planAreaRef.current;
      if (!area) return;

      const scrollLeft = area.scrollLeft;
      const scrollTop = area.scrollTop;
      const containerRect = area.getBoundingClientRect();
      const x = (e.clientX - containerRect.left + scrollLeft) / scale;
      const y = (e.clientY - containerRect.top + scrollTop) / scale;

      addBlock(x, y);
      setMode('move_seat');
    }
  };

  const handleMoveSelected = (dx, dy) => {
    if (mode === 'move_seat') {
      const updated = {
        ...planData,
        koltuklar: planData.koltuklar.map(seat =>
          seat.isSelected ? { ...seat, x: seat.x + dx, y: seat.y + dy } : seat
        )
      };

      const { plan: normalized, shiftX, shiftY } = ensureCanvasFits(updated);
      setPlanData(normalized);
      updateHistory(normalized);

      if ((shiftX && shiftX > 0) || (shiftY && shiftY > 0)) {
        setPendingScroll({ scrollTop: shiftY, scrollLeft: shiftX, apply: true });
      }
    }
  };

  const handleSalonNameChange = (e) => {
    setPlanData(prev => ({ ...prev, salonAdi: e.target.value }));
  };

  const handleReset = () => {
    setPlanData(initialPlan);
    setPlanHistory([initialPlan]);
    setHistoryIndex(0);
    setActivePlanId(null);
    showNotification("Plan sıfırlandı.");
  };

  const showNotification = (message) => {
    setNotification(message);
    setTimeout(() => setNotification(''), 3000);
  };

  // --- API Operations ---

  const savePlan = async () => {
    if (!planData.salonAdi || planData.salonAdi.trim() === '') {
      showNotification("Lütfen bir Salon Adı girin.");
      return;
    }

    const cleanPlanData = {
      ...planData,
      koltuklar: planData.koltuklar.map(({ isSelected, ...rest }) => rest)
    };

    setIsSaving(true);
    try {
      // POST: /api/admin/save-seating-plan
      const response = await fetch(SAVE_PLAN_ENDPOINT, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          salonAdi: planData.salonAdi,
          planAdi: planData.salonAdi,
          planData: cleanPlanData
        })
      });

      const result = await response.json();

      if (response.ok) {
        await fetchSavedPlans();
        setActivePlanId(result.salonAdi);
        showNotification(`✓ Plan başarıyla kaydedildi! (${result.salonAdi})`);
      } else {
        showNotification(`HATA: Kaydedilemedi. Sunucu: ${result.message || response.statusText}`);
      }
    } catch (e) {
      console.error("Plan kaydetme hatası:", e);
      showNotification("Hata: Plan kaydedilemedi. Sunucuya bağlanılamıyor.");
    } finally {
      setIsSaving(false);
    }
  };

  const loadPlan = useCallback(async (salonAdi) => {
    if (!salonAdi) return;

    try {
      // GET: /api/admin/get-seating-plan/{salonAdi}
      const response = await fetch(`${GET_PLAN_ENDPOINT}/${salonAdi}`);
      const result = await response.json();

      if (response.ok) {
        // Controller'da planData JSON.Deserialize edilmiş obje olarak dönüyor
        const loadedData = result.planData;

        // Eğer veritabanından gelen veri formatında eksiklik varsa (örn. eski veri)
        // Varsayılan değerlerle birleştir
        const mergedData = {
          ...initialPlan,
          ...loadedData,
          koltuklar: loadedData.koltuklar || [],
          bloklar: loadedData.bloklar || [],
          sahne: loadedData.sahne || initialPlan.sahne
        };

        setPlanData(mergedData);
        setActivePlanId(result.salonAdi);
        setPlanHistory([mergedData]);
        setHistoryIndex(0);

        showNotification(`Salon '${result.salonAdi}' yüklendi.`);
      } else {
        showNotification(`Hata: Salon yüklenemedi. Sunucu: ${result.message || response.statusText}`);
      }
    } catch (e) {
      console.error("Plan yükleme hatası:", e);
      showNotification(`Hata: Plan yüklenirken bir sorun oluştu.`);
    }
  }, [fetchSavedPlans]);

  // --- UI/Rendering ---

  const renderSeat = (seat) => (
    <div
      key={seat.id}
      className={`absolute flex items-center justify-center rounded-md text-xs cursor-pointer select-none transition-colors duration-100 
        ${seat.isSelected ? 'bg-yellow-500 border-yellow-700' : 'bg-blue-600 hover:bg-blue-500 border-blue-700'} 
        ${seat.status === 'reserved' ? 'bg-red-500 border-red-700' : ''}
        ${seat.blok.toLowerCase().startsWith('m') ? 'bg-gray-700 hover:bg-gray-600 border-gray-800' : ''}
      `}
      style={{
        left: seat.x * scale,
        top: seat.y * scale,
        width: SEAT_SIZE * scale,
        height: SEAT_SIZE * scale,
        fontSize: `${0.65 * scale}rem`,
        borderWidth: 2,
        color: 'white',
        boxShadow: seat.isSelected ? '0 0 5px rgba(255, 255, 0, 0.8)' : 'none',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        lineHeight: '1',
        textAlign: 'center'
      }}
      onClick={() => handleSeatClick(seat.id)}
      title={`${seat.blok} ${seat.sira}${seat.numara}`}
    >
      {seat.sira}{seat.numara}
    </div>
  );

  // Compute canvas size
  const canvasExtent = planData.koltuklar.reduce((acc, s) => {
    acc.maxX = Math.max(acc.maxX, (s.x || 0) + SEAT_SIZE + SEAT_MARGIN);
    acc.maxY = Math.max(acc.maxY, (s.y || 0) + SEAT_SIZE + SEAT_MARGIN);
    return acc;
  }, { maxX: 1296, maxY: 672 });

  const stageBottomY = (planData.sahne.y || 0) + planData.sahne.height;
  const canvasWidth = Math.max(canvasExtent.maxX + 100, planData.sahne.width + 100);
  const canvasHeight = Math.max(canvasExtent.maxY + 100, stageBottomY + 100);

  const scaledCanvasWidth = canvasWidth * scale;
  const scaledCanvasHeight = canvasHeight * scale;

  return (
    <div className="flex h-screen bg-gray-900 text-white font-inter">
      {/* Sol Sidebar - Kontroller */}
      <div className="w-64 bg-gray-800 p-4 flex flex-col shadow-xl">
        <h1 className="text-xl font-bold mb-6 text-yellow-400 border-b border-gray-700 pb-2">Oturma Planı Tasarımı</h1>

        <div className="flex-1 overflow-y-auto -mr-2 pr-2">
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-400 mb-1">Salon Adı</label>
            <input
              type="text"
              value={planData.salonAdi}
              onChange={handleSalonNameChange}
              className="w-full p-2 rounded bg-gray-700 border border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
              placeholder="Örn: Cemal Reşit Rey"
            />
          </div>

          <div className="bg-gray-700 p-3 rounded-lg mb-4">
            <h3 className="text-lg font-semibold mb-3 border-b border-gray-600 pb-2">Blok Ekle</h3>

            <label className="block text-xs font-medium text-gray-400 mb-1 mt-2">Blok Adı (Örn: A, B, M)</label>
            <input
              type="text"
              value={blockName}
              onChange={(e) => setBlockName(e.target.value.toUpperCase())}
              className="w-full p-2 rounded bg-gray-800 border border-gray-600 text-sm"
              maxLength="3"
            />

            <label className="block text-xs font-medium text-gray-400 mb-1 mt-2">Başlangıç Sıra Harfi (Örn: A)</label>
            <input
              type="text"
              value={startLetter}
              onChange={(e) => setStartLetter(e.target.value.toUpperCase())}
              className="w-full p-2 rounded bg-gray-800 border border-gray-600 text-sm"
              maxLength="1"
            />

            <label className="block text-xs font-medium text-gray-400 mb-1 mt-2">Satır Kapasitesi (Satır Başına Koltuk Sayısı)</label>
            <input
              type="number"
              value={rowCapacity}
              onChange={(e) => setRowCapacity(e.target.value)}
              className="w-full p-2 rounded bg-gray-800 border border-gray-600 text-sm"
            />

            <label className="block text-xs font-medium text-gray-400 mb-1 mt-2">Sütun Sayısı (Kaç Satır Yüksekliğinde)</label>
            <input
              type="number"
              value={numRows}
              onChange={(e) => setNumRows(e.target.value)}
              className="w-full p-2 rounded bg-gray-800 border border-gray-600 text-sm"
            />

            <label className="block text-xs font-medium text-gray-400 mb-1 mt-2">Başlangıç Koltuk No (Simetrik İçin İhmal Edilir)</label>
            <input
              type="number"
              value={startNumber}
              onChange={(e) => setStartNumber(e.target.value)}
              className="w-full p-2 rounded bg-gray-800 border border-gray-600 text-sm"
              disabled={direction === 'symmetric' || direction === 'reverse_symmetric'}
            />

            <label className="block text-xs font-medium text-gray-400 mb-1 mt-2">Koltuk Numaralama Yönü</label>
            <select
              value={direction}
              onChange={(e) => setDirection(e.target.value)}
              className="w-full p-2 rounded bg-gray-800 border border-gray-600 text-sm"
            >
              <option value="symmetric">Simetrik (1, 3, 5... | ...6, 4, 2)</option>
              <option value="reverse_symmetric">Ters Simetrik (2, 4, 6... | ...5, 3, 1)</option>
              <option value="left">Soldan Sağa (1, 2, 3...)</option>
              <option value="right">Sağdan Sola (...3, 2, 1)</option>
            </select>

            <button
              onClick={handleBlockAdd}
              className="w-full mt-4 p-2 bg-green-600 hover:bg-green-700 rounded-lg font-semibold transition duration-150"
            >
              <i className="fas fa-cubes mr-2"></i> Blok Ekle
            </button>
          </div>
        </div>

        {/* Koltuk Taşıma Kontrolü */}
        <div className="bg-gray-700 p-3 rounded-lg mt-4">
          <h3 className="text-lg font-semibold mb-3 border-b border-gray-600 pb-2">Koltuk Taşı (Seçili Olanlar)</h3>
          <p className="text-xs text-gray-400 mb-3">Ok tuşlarını kullan (↑↓←→) veya tıkla.</p>
          <div className="flex justify-center space-x-2">
            <button
              onClick={() => handleMoveSelected(0, -5)}
              className="p-2 bg-gray-600 hover:bg-gray-500 rounded-full transition"
            ><i className="fas fa-arrow-up"></i></button>
          </div>
          <div className="flex justify-center space-x-2 mt-2">
            <button
              onClick={() => handleMoveSelected(-5, 0)}
              className="p-2 bg-gray-600 hover:bg-gray-500 rounded-full transition"
            ><i className="fas fa-arrow-left"></i></button>
            <button
              onClick={() => handleMoveSelected(5, 0)}
              className="p-2 bg-gray-600 hover:bg-gray-500 rounded-full transition"
            ><i className="fas fa-arrow-right"></i></button>
          </div>
          <div className="flex justify-center space-x-2 mt-2">
            <button
              onClick={() => handleMoveSelected(0, 5)}
              className="p-2 bg-gray-600 hover:bg-gray-500 rounded-full transition"
            ><i className="fas fa-arrow-down"></i></button>
          </div>
        </div>
      </div>

      {/* Ana Çalışma Alanı */}
      <div className="flex-1 flex flex-col overflow-hidden relative">
        <div className="p-4 bg-gray-800 border-b border-gray-700 flex justify-between items-center z-10">
          <h2 className="text-xl font-light text-white">Plan Düzenleniyor: <span className="font-medium text-blue-300">{planData.salonAdi}</span></h2>
          <div className="flex items-center space-x-3">
            <button
              onClick={handleUndo}
              disabled={historyIndex === 0}
              className="p-2 bg-gray-700 hover:bg-gray-600 disabled:opacity-50 rounded-lg transition"
              title="Geri al (Ctrl+Z)"
            >
              <i className="fas fa-undo"></i>
            </button>
            <button
              onClick={handleRedo}
              disabled={historyIndex >= planHistory.length - 1}
              className="p-2 bg-gray-700 hover:bg-gray-600 disabled:opacity-50 rounded-lg transition"
              title="İleri al (Ctrl+Y)"
            >
              <i className="fas fa-redo"></i>
            </button>
            {notification && (
              <div className="text-sm bg-yellow-600 text-white px-3 py-1 rounded-full animate-pulse">
                {notification}
              </div>
            )}
          </div>
        </div>

        <div
          id="plan-area"
          ref={planAreaRef}
          className="flex-1 relative overflow-auto p-4 border-r border-gray-700"
          onClick={handlePlanAreaClick}
        >
          <div
            id="plan-canvas"
            ref={canvasRef}
            style={{
              position: 'relative',
              width: scaledCanvasWidth,
              height: scaledCanvasHeight,
              backgroundImage: `
                linear-gradient(0deg, transparent calc(${(SEAT_SIZE + SEAT_MARGIN) * scale}px - 1px), rgba(255,255,255,0.05) calc(${(SEAT_SIZE + SEAT_MARGIN) * scale}px - 1px), rgba(255,255,255,0.05) ${(SEAT_SIZE + SEAT_MARGIN) * scale}px),
                linear-gradient(90deg, transparent calc(${(SEAT_SIZE + SEAT_MARGIN) * scale}px - 1px), rgba(255,255,255,0.05) calc(${(SEAT_SIZE + SEAT_MARGIN) * scale}px - 1px), rgba(255,255,255,0.05) ${(SEAT_SIZE + SEAT_MARGIN) * scale}px)
              `,
              backgroundSize: `${(SEAT_SIZE + SEAT_MARGIN) * scale}px ${(SEAT_SIZE + SEAT_MARGIN) * scale}px`,
              backgroundPosition: '0 0'
            }}
          >
            <div
              className="absolute bg-red-800 text-white flex items-center justify-center rounded-b-lg shadow-xl"
              style={{
                width: planData.sahne.width * scale,
                height: planData.sahne.height * scale,
                left: '50%',
                transform: 'translateX(-50%)',
                bottom: planData.sahne.y * scale,
              }}
            >
              <span className="font-bold text-lg">SAHNE</span>
            </div>

            {planData.koltuklar.map(renderSeat)}
          </div>
        </div>

        <div className="p-4 bg-gray-800 border-t border-gray-700 flex flex-col">
          <div className="mb-3">
            <h3 className="text-sm font-semibold text-gray-400 mb-1">Kayıtlı Salonlar (API)</h3>
            <div className="flex flex-wrap gap-2 text-sm max-h-24 overflow-y-auto p-1 bg-gray-700 rounded-md">
              {savedPlans.length > 0 ? (
                savedPlans.map(plan => (
                  <button
                    key={plan.id}
                    onClick={() => loadPlan(plan.salonAdi)}
                    className={`px-2 py-1 rounded text-xs font-medium transition duration-150 ${activePlanId === plan.salonAdi ? 'bg-blue-600 text-white' : 'bg-gray-600 hover:bg-gray-500 text-gray-100'}`}
                    title={plan.salonAdi}
                  >
                    {plan.salonAdi}
                  </button>
                ))
              ) : (
                <p className="text-gray-400 text-xs">Henüz kayıtlı salon planı yok.</p>
              )}
            </div>
          </div>

          <div className="flex gap-4">
            <button
              onClick={savePlan}
              disabled={isSaving}
              className="flex-1 p-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-bold transition duration-150 disabled:opacity-50"
            >
              <i className="fas fa-save mr-2"></i> {isSaving ? 'Kaydediliyor...' : 'Planı Kaydet (Veritabanı)'}
            </button>
            <button
              onClick={() => downloadJson(planData)}
              className="p-3 bg-yellow-600 hover:bg-yellow-700 rounded-lg font-bold transition duration-150"
            >
              <i className="fas fa-download mr-2"></i> JSON Olarak İndir
            </button>
            <button
              onClick={handleReset}
              className="p-3 bg-red-600 hover:bg-red-700 rounded-lg font-bold transition duration-150"
            >
              <i className="fas fa-trash-alt mr-2"></i> Sıfırla
            </button>
          </div>
        </div>
      </div>

      {/* Sağ Sidebar - Satır Seçim Kontrolleri */}
      <div className="w-64 bg-gray-800 p-4 flex flex-col shadow-xl border-l border-gray-700 overflow-y-auto">
        <h2 className="text-lg font-bold mb-4 text-yellow-400 border-b border-gray-700 pb-2">Seçim Kontrolleri</h2>

        {planData.koltuklar.length > 0 && (
          <div className="bg-gray-700 p-3 rounded-lg mb-4 border border-gray-600">
            <p className="text-sm font-bold text-yellow-400 mb-2">Salon Bilgisi</p>
            <p className="text-xs text-gray-300">Toplam Koltuk: <span className="font-semibold text-yellow-300">{planData.koltuklar.length}</span></p>
            <p className="text-xs text-gray-300">Blok Sayısı: <span className="font-semibold text-yellow-300">{planData.bloklar.length}</span></p>
            <p className="text-xs text-gray-400 mt-2">Satır Sayısı: <span className="font-semibold text-yellow-300">{Math.max(...planData.bloklar.map(b => b.rows), 0)}</span></p>
          </div>
        )}

        <div className="bg-gray-700 p-3 rounded-lg mb-4">
          <button
            onClick={handleSelectAll}
            className="w-full p-2 bg-yellow-600 hover:bg-yellow-700 rounded-lg font-semibold text-sm mb-2 transition"
            title="Tüm koltukları seç (Ctrl+A)"
          >
            <i className="fas fa-check-double mr-2"></i> Hepsini Seç
          </button>
          <button
            onClick={handleDeselectAll}
            className="w-full p-2 bg-gray-600 hover:bg-gray-500 rounded-lg font-semibold text-sm transition"
            title="Tüm seçimleri kaldır (Ctrl+D)"
          >
            <i className="fas fa-times mr-2"></i> Seçimi Kaldır
          </button>
          <button
            onClick={handleDeleteSelected}
            className="w-full p-2 bg-red-600 hover:bg-red-700 rounded-lg font-semibold text-sm mt-2 transition"
            title="Seçili koltukları sil"
          >
            <i className="fas fa-trash-alt mr-2"></i> Seçili Sil
          </button>
        </div>

        {planData.bloklar.length > 0 && (
          <div className="bg-gray-700 p-3 rounded-lg">
            <h3 className="text-sm font-semibold text-gray-400 mb-3 border-b border-gray-600 pb-2">Satırlar</h3>
            <div className="space-y-3">
              {planData.bloklar.map((blok) => (
                <div key={blok.blokAdi} className="bg-gray-800 p-2 rounded-lg">
                  <p className="text-xs font-bold text-yellow-400 mb-2">{blok.blokAdi}</p>
                  <div className="flex flex-wrap gap-1">
                    {Array.from({ length: blok.rows }, (_, i) => {
                      const startLtrIndex = blok.startLetter.toUpperCase().charCodeAt(0) - 65;
                      const rowLetter = String.fromCharCode(65 + startLtrIndex + i);
                      const rowSeats = planData.koltuklar.filter(
                        s => s.blok === blok.blokAdi && s.sira === rowLetter
                      );

                      if (rowSeats.length === 0) return null;

                      const allSelected = rowSeats.length > 0 && rowSeats.every(s => s.isSelected);

                      return (
                        <button
                          key={`${blok.blokAdi}${rowLetter}`}
                          onClick={() => handleSelectRow(blok.blokAdi, rowLetter)}
                          className={`px-2 py-1 text-xs font-semibold rounded transition ${allSelected
                            ? 'bg-yellow-500 text-gray-900'
                            : 'bg-gray-600 hover:bg-gray-500 text-white'
                            }`}
                          title={`${blok.blokAdi}${rowLetter} satırını seç/bırak`}
                        >
                          {rowLetter}
                        </button>
                      );
                    })}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {planData.koltuklar.length === 0 && (
          <div className="bg-gray-700 p-3 rounded-lg text-center text-gray-400 text-xs mt-auto">
            <p>Blok ekleyerek başlayın.</p>
            <p className="mt-2 text-gray-500 text-xs">Kısa Yollar:</p>
            <p className="text-yellow-300 text-xs">Ctrl+A: Hepsini Seç</p>
            <p className="text-yellow-300 text-xs">Ctrl+D: Seçimi Kaldır</p>
          </div>
        )}

        {planData.koltuklar.length > 0 && (
          <div className="bg-gray-700 p-3 rounded-lg mt-auto border-t border-gray-600 pt-3">
            <p className="text-xs font-semibold text-gray-400">İstatistikler</p>
            <p className="text-sm text-yellow-400 font-bold mt-1">
              Seçili: {planData.koltuklar.filter(s => s.isSelected).length} / {planData.koltuklar.length}
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default App;