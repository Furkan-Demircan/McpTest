// Uygulama manifest'ini sunucuya aktarır ve kaymaları yakalar.
//
//   npm run manifest         → server/src/MCP/Manifest/app-manifest.json'u üretir
//   npm run manifest:check   → üretmeden doğrular (CI için); fark/hata varsa exit 1
//
// Doğrulamalar:
//   1. Manifest kendi içinde tutarlı mı (kimlik tekrarları, form ↔ sayfa bağları)
//   2. Manifest'teki her eleman kimliği JSX'te gerçekten var mı (id="...")
//   3. Knowledge/*.md içindeki `pages:` ve [@elemanId] referansları manifest'te var mı
//   4. (check) Commit'lenmiş JSON manifest'in güncel hali mi
//
// Node 24'ün type stripping desteğiyle TS manifest doğrudan import edilir.

import { readFileSync, writeFileSync, readdirSync, existsSync } from 'node:fs'
import { join, relative, dirname, sep } from 'node:path'
import { fileURLToPath } from 'node:url'
import { appManifest } from '../src/app/appManifest.ts'

const clientRoot = join(dirname(fileURLToPath(import.meta.url)), '..')
const repoRoot = join(clientRoot, '..')
const outputPath = join(repoRoot, 'server/src/MCP/Manifest/app-manifest.json')
const knowledgeDir = join(repoRoot, 'server/src/MCP/Knowledge')
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
const pageIds = appManifest.pages.map((page) => page.id)
const formIds = appManifest.forms.map((form) => form.id)
const paths = appManifest.pages.flatMap((page) => [page.path, ...page.aliases])

const elementIds = [
  ...appManifest.pages.flatMap((page) => page.elements.map((element) => element.id)),
  ...appManifest.forms.flatMap((form) => form.fields.map((field) => field.elementId)),
]

for (const id of duplicates(pageIds)) errors.push(`Tekrarlanan sayfa kimliği: ${id}`)
for (const id of duplicates(formIds)) errors.push(`Tekrarlanan form kimliği: ${id}`)
for (const path of duplicates(paths)) errors.push(`Tekrarlanan path/alias: ${path}`)
for (const id of duplicates(elementIds)) errors.push(`Tekrarlanan eleman kimliği: ${id}`)

for (const form of appManifest.forms) {
  const page = appManifest.pages.find((item) => item.id === form.pageId)
  if (!page) errors.push(`Form '${form.id}' olmayan sayfaya bağlı: ${form.pageId}`)
  else if (page.formId !== form.id) errors.push(`Sayfa '${page.id}' formId'si '${form.id}' olmalı`)

  for (const id of [form.submitElementId, form.resetElementId]) {
    if (!page?.elements.some((element) => element.id === id)) {
      errors.push(`Form '${form.id}' için '${id}' sayfa elemanlarında tanımlı değil`)
    }
  }
}

// 2. Manifest ↔ JSX
const jsxIds = new Set(
  walk(join(clientRoot, 'src'), '.tsx').flatMap((file) =>
    [...readFileSync(file, 'utf8').matchAll(/\bid="([^"]+)"/g)].map((match) => match[1])
  )
)

for (const id of elementIds) {
  if (!jsxIds.has(id)) errors.push(`Manifest elemanı JSX'te yok: id="${id}"`)
}

// 3. Bilgi tabanı ↔ manifest
const knownElements = new Set(elementIds)
const knownPages = new Set(pageIds)

for (const file of existsSync(knowledgeDir) ? walk(knowledgeDir, '.md') : []) {
  const text = readFileSync(file, 'utf8')
  const pagesLine = text.match(/^pages:\s*(.*)$/m)?.[1] ?? ''

  for (const pageId of pagesLine.split(',').map((item) => item.trim()).filter(Boolean)) {
    if (!knownPages.has(pageId)) errors.push(`${rel(file)}: bilinmeyen sayfa '${pageId}'`)
  }

  for (const [, elementId] of text.matchAll(/\[@([\w-]+)\]/g)) {
    if (!knownElements.has(elementId)) errors.push(`${rel(file)}: bilinmeyen eleman [@${elementId}]`)
  }
}

// 4. Üret / karşılaştır
const json = JSON.stringify(appManifest, null, 2) + '\n'
const current = existsSync(outputPath) ? readFileSync(outputPath, 'utf8').replace(/\r\n/g, '\n') : null

if (checkOnly) {
  if (current !== json) errors.push(`${rel(outputPath)} güncel değil; 'npm run manifest' çalıştırın`)
} else if (errors.length === 0 && current !== json) {
  writeFileSync(outputPath, json)
  console.log(`✔ ${rel(outputPath)} güncellendi`)
}

if (errors.length > 0) {
  console.error(`✖ Manifest doğrulaması başarısız (${errors.length}):`)
  for (const error of errors) console.error(`  - ${error}`)
  process.exit(1)
}

console.log(
  `✔ Manifest geçerli: ${pageIds.length} sayfa, ${formIds.length} form, ${elementIds.length} eleman`
)
