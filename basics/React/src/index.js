import React from 'react';
import { createRoot } from 'react-dom/client';
// App.jsx dosyanızdaki ana bileşeni import edin
import App from './App.jsx'; 

const container = document.getElementById('react-root');
if (container) {
  // React 18 ile yeni başlatma yöntemi
  const root = createRoot(container); 
  root.render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
} else {
    console.error("HTML içinde 'react-root' ID'li bir element bulunamadı. Lütfen .cshtml dosyanızı kontrol edin.");
}