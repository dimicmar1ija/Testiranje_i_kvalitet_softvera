# 🎬 Filmski Kutak

Grupni projekat iz predmeta **Napredne baze podataka**.  
Filmski Kutak je web forum namenjen ljubiteljima filma, gde korisnici mogu da razmenjuju mišljenja, objavljuju postove i komentare, kao i da prate aktivnosti drugih članova zajednice.  

---

## ✨ Funkcionalnosti

- 👤 **Korisnici**
  - Registracija i prijava korisnika (JWT autentifikacija i autorizacija)  
  - Lozinke se čuvaju heširane pomoću **bcrypt** biblioteke  
  - Pretraga profila drugih korisnika  
  - Izmena i brisanje sopstvenog profila  
  - Posebna uloga **admina** sa dodatnim privilegijama  

- 📝 **Postovi**
  - Kreiranje, izmena i brisanje postova  
  - Dodavanje **fotografija** i **video sadržaja** uz post  
  - Dodavanje **tagova** i **lajkova**  

- 💬 **Komentari**
  - Ugnježdeni komentari (odgovori na komentare)  

---

## 🛠️ Tehnologije

- **Backend**: C# / ASP.NET Core  
- **Autentifikacija**: JWT (JSON Web Tokens)  
- **Sigurnost**: bcrypt (heširanje lozinki)  
- **Baza podataka**: MongoDB
- **Frontend**: React   

--- 

## 📸 Screenshotovi aplikacije

### 👤 Profil korisnika
![Profil](./screenshots/profile.png)

### 📝 Pregled posta sa komentarima
![Post](./screenshots/post.png)

### ➕ Kreiranje posta
![Kreiranje posta](./screenshots/createPost.png)

