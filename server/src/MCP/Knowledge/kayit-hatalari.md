---
id: kayit-hatalari
title: Kayıt sırasında alınan hatalar ve çözümleri
pages: student-create, teacher-create, users
keywords: hata, hatası, kaydedilmiyor, kaydolmuyor, olmuyor, uyarı, kırmızı, zorunlu, geçersiz, mevcut, zaten, tc, e-posta, çalışmıyor, sorun
---
Form kaydedilmiyorsa hatanın nerede göründüğüne bakın.

## Alanın altında kırmızı uyarı
Alan boş bırakılmış veya kurala uymuyor (örn. TC No 11 hane değil, ya da doğum tarihi makul yaş aralığında değil: öğrenci 5–25, öğretmen 18–70 yaş). Uyarıdaki mesaj neyin eksik olduğunu söyler; ilgili alanı düzeltip tekrar kaydedin. Alanların kuralları için sayfa şemasına bakılabilir.

## Sayfanın üstünde kırmızı bant (sunucu hatası)
- "... TC Kimlik Numarası ile kayıtlı bir kullanıcı zaten mevcut": Kişi daha önce kaydedilmiş. Kayıtlı Kullanıcılar sayfasında [@usersSearch] ile TC No'yu arayarak kontrol edin; tekrar kaydetmeyin.
- "... e-posta adresi ile kayıtlı bir kullanıcı zaten mevcut": E-posta başka bir kayıtta kullanılıyor; kişinin başka bir e-postasını girin.
- "Sunucuya bağlanırken bir hata oluştu": Sunucu veya veritabanı erişilemiyor. Girdiğiniz bilgiler formda durur; bir süre sonra tekrar deneyin, sürerse sistem yöneticisine bildirin.
