// Asistanın sayfa kataloğunu sunucuya aktarır ve kaymaları yakalar.
//
//   npm run pages         → server/src/MCP/Manifest/app-pages.json'u üretir
//   npm run pages:check   → üretmeden doğrular (CI için); fark/hata varsa exit 1
//
// Doğrulamalar:
//   1. Katalog kendi içinde tutarlı mı (tekrarlanan sayfa kimliği / path)
//   2. Katalogdaki path ve alias'lar App.tsx route'larında var mı
//   3. Katalogdaki eleman kimlikleri JSX'te gerçekten var mı (id="...")
//   4. Knowledge/*.md içindeki `pages:` kimlikleri katalogda var mı
//   5. (check) Commit'lenmiş JSON güncel mi
//
// Form alanları burada doğrulanmaz: onlar Swagger'dan gelir, sunucu açılışta
// endpoint'leri ve rehberlerdeki [@alan] referanslarını doğrular.

import { readFileSync, writeFileSync, readdirSync, existsSync, mkdirSync } from 'node:fs'
import { join, relative, dirname, sep } from 'node:path'
import { fileURLToPath } from 'node:url'
import { aiPages } from '../src/app/aiPages.ts'

const clientRoot = join(dirname(fileURLToPath(import.meta.url)), '..')
const repoRoot = join(clientRoot, '..')
const outputPath = join(repoRoot, 'server/src/MCP/Manifest/app-pages.json')
const knowledgeDir = join(repoRoot, 'server/src/MCP/Knowledge')
const appPath = join(clientRoot, 'src/App.tsx')
const checkOnly = process.argv.includes('--check')

const errors = []
const rel = (path) => relative(repoRoot, path).split(sep).join('/')

function walk(dir, extension) {
  return readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const path = join(dir, entry.name)
    if (entry.isDirectory()) return walk(path, extension)
    return path.endsWith(extension) ? [path] : []
  })
}

function duplicates(values) {
  return [...new Set(values.filter((value, index) => values.indexOf(value) !== index))]
}

// 1. İç tutarlılık
const pageIds = aiPages.map((page) => page.id)
const paths = aiPages.flatMap((page) => [page.path, ...page.aliases])

for (const id of duplicates(pageIds)) errors.push(`Tekrarlanan sayfa kimliği: ${id}`)
for (const path of duplicates(paths)) errors.push(`Tekrarlanan path/alias: ${path}`)

for (const page of aiPages) {
  if (page.endpoint && !/^(GET|POST|PUT|PATCH|DELETE) \/\S+$/i.test(page.endpoint)) {
    errors.push(`Sayfa '${page.id}': endpoint "METHOD /yol" biçiminde olmalı: ${page.endpoint}`)
  }
}

// 2. Katalog ↔ route'lar
const routePaths = new Set([...readFileSync(appPath, 'utf8').matchAll(/\bpath="([^"]+)"/g)].map((match) => match[1]))
for (const path of paths) {
  if (!routePaths.has(path)) errors.push(`Katalogdaki path App.tsx route'larında yok: ${path}`)
}

// 3. Katalog ↔ JSX eleman kimlikleri
const jsxIds = new Set(
  walk(join(clientRoot, 'src'), '.tsx').flatMap((file) =>
    [...readFileSync(file, 'utf8').matchAll(/\bid="([^"]+)"/g)].map((match) => match[1])
  )
)
for (const page of aiPages) {
  for (const element of page.elements) {
    if (!jsxIds.has(element.id)) errors.push(`Sayfa '${page.id}': eleman JSX'te yok: id="${element.id}"`)
  }
}

// 4. Rehberler ↔ katalog
const knownPages = new Set(pageIds)
for (const file of existsSync(knowledgeDir) ? walk(knowledgeDir, '.md') : []) {
  const pagesLine = readFileSync(file, 'utf8').match(/^pages:\s*(.*)$/m)?.[1] ?? ''
  for (const pageId of pagesLine.split(',').map((item) => item.trim()).filter(Boolean)) {
    if (!knownPages.has(pageId)) errors.push(`${rel(file)}: bilinmeyen sayfa '${pageId}'`)
  }
}

// 5. Üret / karşılaştır
const json = JSON.stringify({ pages: aiPages }, null, 2) + '\n'
const current = existsSync(outputPath) ? readFileSync(outputPath, 'utf8').replace(/\r\n/g, '\n') : null

if (checkOnly) {
  if (current !== json) errors.push(`${rel(outputPath)} güncel değil; 'npm run pages' çalıştırın`)
} else if (errors.length === 0 && current !== json) {
  mkdirSync(dirname(outputPath), { recursive: true })
  writeFileSync(outputPath, json)
  console.log(`✔ ${rel(outputPath)} güncellendi`)
}

if (errors.length > 0) {
  console.error(`✖ Sayfa kataloğu doğrulaması başarısız (${errors.length}):`)
  for (const error of errors) console.error(`  - ${error}`)
  process.exit(1)
}

const withEndpoint = aiPages.filter((page) => page.endpoint).length
console.log(`✔ Sayfa kataloğu geçerli: ${aiPages.length} sayfa (${withEndpoint} form endpoint'i)`)
