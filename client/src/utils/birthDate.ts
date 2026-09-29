/**
 * Doğum tarihi için makul yaş aralığı kontrolü. Aralıklar backend DTO'larıyla aynıdır
 * (CreateUserDto: 5–25, CreateTeacherDto: 18–70); asıl doğrulama sunucudadır,
 * bu sadece kullanıcıya erken geri bildirim ve tarih seçicinin sınırları içindir.
 */

function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

/** YYYY-MM-DD doğum tarihine göre bugünkü yaş; tarih geçersizse null. */
export function ageOn(isoDate: string, today = new Date()): number | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(isoDate)
  if (!match) return null

  const [year, month, day] = match.slice(1).map(Number)
  let age = today.getFullYear() - year
  if (today.getMonth() + 1 < month || (today.getMonth() + 1 === month && today.getDate() < day)) age--
  return age
}

/** Tarih seçicinin min/max değerleri: [bugün − (maxAge+1) yıl + 1 gün, bugün − minAge yıl] */
export function birthDateBounds(minAge: number, maxAge: number, today = new Date()) {
  const latest = new Date(today.getFullYear() - minAge, today.getMonth(), today.getDate())
  const earliest = new Date(today.getFullYear() - maxAge - 1, today.getMonth(), today.getDate() + 1)
  return { min: toIsoDate(earliest), max: toIsoDate(latest) }
}

/** Yaş aralık dışındaysa hata mesajı, değilse undefined. */
export function birthDateError(isoDate: string, minAge: number, maxAge: number, who: string): string | undefined {
  const age = ageOn(isoDate)
  if (age === null) return 'Geçerli bir doğum tarihi giriniz.'
  if (age < minAge || age > maxAge) {
    return `${who} yaşı ${minAge} ile ${maxAge} arasında olmalıdır; doğum tarihini kontrol edin.`
  }
  return undefined
}
