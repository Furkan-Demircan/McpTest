using System.ComponentModel.DataAnnotations;
using Application.Forms;

namespace Application.DTOs;

public record CreateTeacherDto : CreateUserDto
{
    [Display(Name = "Branş / Uzmanlık Alanı", Order = 50,
        Description = "Elle yazılabilir veya öneri listesinden seçilebilir. Şu an veritabanına kaydedilmez.")]
    [Required(ErrorMessage = "Öğretmen branş / uzmanlık alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Branş en fazla 100 karakter olabilir.")]
    [Suggestions(
        "Matematik", "Fizik", "Kimya", "Biyoloji", "Türkçe ve Edebiyat", "Tarih", "Coğrafya",
        "İngilizce", "Bilişim Teknolojileri / Yazılım", "Müzik", "Görsel Sanatlar", "Beden Eğitimi",
        "Felsefe", "Rehberlik ve Psikolojik Danışmanlık", "Sınıf Öğretmenliği")]
    public string Branch { get; init; } = default!;
}
