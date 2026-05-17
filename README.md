# 1000X

Game Bullet Hell shooter minimalis yang dibangun menggunakan Unity, fokus pada efisiensi performa tinggi dan arsitektur modular

---

## Informasi Game

### Kontrol & Navigasi

- **Movemenet**: WASD / Tombol Arah
- **Shooting**: Spasi / Klik Kiri (Tahan)
- **Pause**: Escape
- **Navigasi UI**: Menggunakan Mouse untuk berinteraksi dengan tombol menu

### Win & Lose Conditions

- **Win Condition**: Survive selama 5 menit dan mengalahkan Boss
- **Lose Condition**: HP Player mencapai 0 akibat terkena proyektil atau menabrak musuh

---

## Fitur

### Fitur yang Berhasil Diimplementasikan

- **Object Pooling**: Sistem untuk menghindari penggunaan Instantiate/Destroy yang dapat menyebabkan lag
- **Data Driven menggunakan ScriptableObject**: Statistik musuh dan senjata dikelola melalui ScriptableObjects untuk mempermudah balancing tanpa mengubah kode program
- **Dynamic Difficulty**: Intensitas spawning musuh meningkat seiring waktu menggunakan sistem weighted random
- **Overheat Mode**: Game mode yang menambahkan elemen taktis agar pemain tidak menembak secara terus-menerus tanpa strategi
- **Multi-Phase Boss**: Boss dengan 2 fase yang akan berubah berdasarkan sisa HP
- **Local Leaderboard**: Penyimpanan skor tertinggi menggunakan PlayerPrefs

### Fitur yang Tidak Selesai & Alasannya

- **Sistem Power-up/roguelike**: Rencana awal untuk menambahkan power-up dan sistem roguelike ditunda karena keterbatasan waktu pengerjaan. Prioritas dialihkan untuk menyempurnakan core gameplay loop dan memastikan stabilitas sistem utama

---

## Refleksi

### Known Bugs & Limitasi

- **Kompatibilitas Perangkat**: Game sejauh ini baru diuji di device pribadi. Performa serta kecocokan skala UI pada berbagai spesifikasi perangkat atau rasio layar yang berbeda belum di tes

### Rencana Perbaikan (Future Improvements)

- **Sistem Power-up & Roguelike**: Mengimplementasikan berbagai buff yang dapat diambil untuk meningkatkan kedalaman variasi gameplay.
- **Variasi Musuh**: Menambahkan jenis musuh yang lebih kompleks dengan pola gerakan dan tembakan unik untuk meningkatkan tantangan, terutama boss

### Tantangan Terbesar

Tantangan terbesar selama proses pengerjaan adalah merapikan arsitektur kode dan manajemen pengerjaan. Karena saya terus mencoba ekplorasi fitur baru, padahal core gameplay nya saja belum matang, sehingga terkadang saya harus merombak ulang

---
