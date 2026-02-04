export function calculateAge(birthDateIso) {
  if (!birthDateIso) {
    return 0
  }

  const birthDate = new Date(`${birthDateIso}T00:00:00`)
  if (Number.isNaN(birthDate.getTime())) {
    return 0
  }

  const today = new Date()
  let age = today.getFullYear() - birthDate.getFullYear()
  const monthDiff = today.getMonth() - birthDate.getMonth()

  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
    age -= 1
  }

  return age < 0 ? 0 : age
}

export function formatDate(isoDate) {
  if (!isoDate) {
    return '-'
  }

  const d = new Date(`${isoDate}T00:00:00`)
  if (Number.isNaN(d.getTime())) {
    return '-'
  }

  return d.toLocaleDateString('th-TH')
}