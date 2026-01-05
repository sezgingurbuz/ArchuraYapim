# Archura Yapım - Bilet Satış ve Etkinlik Yönetim Sistemi Proje Raporu

## 1. Proje Tanımı
**Archura Yapım Bilet Satış Sistemi**, tiyatro, konser ve benzeri etkinlikler için geliştirilmiş, kapsamlı bir bilet satış ve yönetim platformudur. Sistem, son kullanıcıların (müşterilerin) online bilet alabilmesine olanak tanırken, yöneticilerin etkinlikleri, salonları ve satış raporlarını detaylı bir şekilde yönetebileceği gelişmiş bir yönetim paneli sunar.

Proje, modern web teknolojileri kullanılarak, güvenli, ölçeklenebilir ve kullanıcı dostu bir deneyim sunmak üzere tasarlanmıştır.

## 2. Kullanılan Teknolojiler ve Araçlar
Proje geliştirme sürecinde endüstri standardı teknolojiler ve kütüphaneler tercih edilmiştir:

*   **Programlama Dili:** C#
*   **Framework:** .NET 8.0 (ASP.NET Core MVC)
*   **Veritabanı:** MySQL (Veritabanı erişimi için Entity Framework Core kullanılmıştır)
*   **ORM (Object-Relational Mapping):** Entity Framework Core (Code-First yaklaşımı)
*   **Kimlik Doğrulama (Authentication):**
    *   ASP.NET Core Identity (Cookie Authentication)
    *   Çoklu Şema Yapısı (Admin ve Müşteri için ayrıştırılmış oturum yönetimi)
    *   Google ile Giriş (OAuth 2.0)
*   **Güvenlik:**
    *   BCrypt (Şifreleme ve parola güvenliği için)
    *   HTTPS Yönlendirmesi
    *   HSTS (HTTP Strict Transport Security)
*   **Ön Yüz (Frontend):**
    *   HTML5, CSS3
    *   JavaScript
    *   Bootstrap (Responsive tasarım için)
*   **Diğer Kütüphaneler:**
    *   **Pomelo.EntityFrameworkCore.MySql:** MySQL entegrasyonu için.
    *   **QRCoder:** Biletler için QR kod oluşturma.

## 3. Mimari Yapı
Proje **MVC (Model-View-Controller)** mimari deseni üzerine inşa edilmiştir. Bu yapı, projenin yönetilebilirliğini artırmak ve kod tekrarını önlemek amacıyla katmanlı bir yapıda kurgulanmıştır.

*   **Model:** Veritabanı tablolarını temsil eden sınıflar (Entities) ve veri erişim katmanı (`ApplicationDbContext`).
*   **View:** Kullanıcı arayüzünü oluşturan `.cshtml` dosyaları. Razor View Engine kullanılarak dinamik içerik üretimi sağlanmıştır.
*   **Controller:** Gelen istekleri karşılayan, iş mantığını çalıştıran ve uygun View'ı kullanıcıya döndüren sınıflar.

Ayrıca proje, yönetici işlemlerini ayrıştırmak için **Areas** yapısını kullanmaktadır. `Admin` alanı (Area), yönetim paneli ile ilgili tüm Controller ve View dosyalarını ayrı bir modül olarak barındırır.

## 4. Temel Özellikler ve Modüller

### 4.1. Müşteri Paneli (Ön Yüz)
*   **Etkinlik Listeleme:** Güncel etkinliklerin listelenmesi ve detaylı gösterimi.
*   **Koltuk Seçimi:** Salon planı üzerinden interaktif koltuk seçimi.
*   **Bilet Satın Alma:** Seçilen koltuklar için ödeme adımları (Kredi Kartı entegrasyonu hazırlığı ve Gişe modu).
*   **Üyelik Sistemi:** Kayıt olma, giriş yapma (Google ile giriş desteği) ve profil yönetimi.
*   **QR Kodlu Bilet:** Satın alınan biletlerin QR kod ile dijital olarak görüntülenmesi.

### 4.2. Yönetim Paneli (Admin Area)
*   **Dashboard:** Satış özetleri ve sistem durumu.
*   **Etkinlik Yönetimi:** Yeni etkinlik ekleme, düzenleme, pasife alma ve arşivleme.
*   **Salon Yönetimi:** Dinamik salon planı oluşturma, koltuk düzenleme ve kapasite yönetimi.
*   **Satış ve Raporlama:**
    *   Detaylı satış raporları (Platform bazlı: Bubilet, Biletinial vb.).
    *   Ödeme yöntemine göre filtreleme.
    *   Platform bazlı koltuk renklendirme (Görsel raporlama).
*   **Kullanıcı Yönetimi:** Admin ve personel yetkilendirmeleri.
*   **Gişe Modu:** Fiziksel satış noktaları için hızlı satış arayüzü.

## 5. Veritabanı Yapısı
Proje, ilişkisel bir veritabanı yapısına sahiptir. `ApplicationDbContext` üzerinde tanımlanan temel tablolar şunları içerir:
*   **Users:** Kullanıcı bilgileri (Admin ve Müşteriler).
*   **Events:** Etkinlik detayları (Ad, Tarih, Açıklama, Görsel vb.).
*   **Halls (Salonlar):** Salon bilgileri ve oturma düzenleri.
*   **Tickets:** Satılan biletler, koltuk numaraları, fiyat ve müşteri ilişkileri.
*   **Sales:** Satış işlemleri ve ödeme detayları.

## 6. Sonuç ve Kazanımlar
Bu proje ile birlikte:
*   Modern bir B2C (Business-to-Consumer) e-ticaret uygulaması geliştirilmiştir.
*   Kurumsal mimariye uygun, genişletilebilir bir kod tabanı oluşturulmuştur.
*   Çoklu rol yönetimi ve güvenlik standartları (KVKK uyumlu şifreleme vb.) uygulanmıştır.
*   Gerçek hayat senaryolarına (Gişe satışı, online satış, raporlama) uygun çözümler üretilmiştir.
