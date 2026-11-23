# Detail interior dari empat foto pengguna

Scene: Assets/Scenes/ITPLNCampusPLTS.unity.

Tambahan: rakitan inverter Huawei, isolator dengan pegangan putar, kotak komunikasi, ACDB dengan meter, trunking, kabel, papan aksesori berlubang dan contoh konektor, label serta papan Sembcorp. Rakitan baru menempati bagian kosong dinding timur. Lokasi ini adalah perkiraan karena foto close-up tidak menunjukkan posisi global ruangan.

Panel hibrida biru diperinci menjadi 13 rumah meter analog dengan muka bersumber dari foto, soket dummy load, tombol dan rocker. Dua diagram Sunny Island OFF LINE dan Fuel Save Controller ON LINE dipasang pada papan informasi dari foto asli pengguna. Foto dipertahankan tanpa modifikasi di Assets/PLTS/InteriorPhotoDetails/References; pemetaan UV memilih bagian relevan.

Ini rekonstruksi visual, bukan jaminan kemiripan 100% atau rangkaian listrik terverifikasi. Dimensi dan kedalaman diperkirakan. Jarum dan tombol baru belum memiliki interaksi runtime. Diagram mengandung pantulan dan keterbatasan ketajaman dari sumber asli.

Builder: DetailInteriorPhotos.Build, lalu DetailInteriorPhotos.AlignMeters, lalu FinishPhotoDetails.Run. Jalankan satu kali pada scene sebelum perubahan; builder mencegah duplikasi. Backup: Backups/ITPLNCampusPLTS-before-photo-details.unity. Seluruh scene, folder asset beserta .meta perlu masuk commit bersama.
