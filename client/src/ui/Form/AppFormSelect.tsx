import { FormControl, FormHelperText, InputLabel, MenuItem, Select } from '@mui/material'
import type { ReactNode } from 'react'
import { useController, type Control, type FieldPath, type FieldValues } from 'react-hook-form'
import { useAiField } from '../ai/useAiField'

export interface AppFormSelectOption {
  value: string | number
  label: ReactNode
}

export interface AppFormSelectProps<TFieldValues extends FieldValues> {
  name: FieldPath<TFieldValues>
  control: Control<TFieldValues>
  label: string
  options: AppFormSelectOption[]
  /** Boş seçenek (örn. "Seçiniz") gösterilir; seçildiğinde değer null olur */
  emptyOptionLabel?: string
  required?: boolean
  disabled?: boolean
}

const normalize = (text: string) => text.trim().toLocaleLowerCase('tr-TR')

export const AppFormSelect = <TFieldValues extends FieldValues>({
  name,
  control,
  label,
  options,
  emptyOptionLabel,
  required,
  disabled,
}: AppFormSelectProps<TFieldValues>) => {
  const { field, fieldState } = useController({ name, control })
  const labelOf = (option: AppFormSelectOption) => String(option.label)

  // Asistan: değeri veya etiketi kabul eder; seçeneklerde yoksa nedenini söyler
  const aiProps = useAiField(name, {
    label,
    kind: 'select',
    required,
    getValue: () => {
      const selected = options.find((option) => option.value === field.value)
      return selected ? labelOf(selected) : undefined
    },
    options: () => options.map((option) => ({ value: option.value, label: labelOf(option) })),
    write: (value) => {
      const text = normalize(String(value ?? ''))
      const match = options.find(
        (option) => normalize(String(option.value)) === text || normalize(labelOf(option)) === text
      )
      if (!match) {
        return {
          ok: false,
          reason: `'${String(value)}' seçeneklerde yok. Seçenekler: ${options.map(labelOf).join(', ')}`,
        }
      }
      field.onChange(match.value)
      return { ok: true }
    },
  })

  return (
    <FormControl {...aiProps} fullWidth size="small" error={!!fieldState.error} required={required}>
      <InputLabel>{label}</InputLabel>
      <Select
        name={field.name}
        inputRef={field.ref}
        onBlur={field.onBlur}
        label={required ? `${label} *` : label}
        value={field.value ?? ''}
        onChange={(event) => field.onChange(event.target.value === '' ? null : event.target.value)}
        disabled={disabled}
      >
        {emptyOptionLabel && (
          <MenuItem value="">
            <em>{emptyOptionLabel}</em>
          </MenuItem>
        )}
        {options.map((option) => (
          <MenuItem key={option.value} value={option.value}>
            {option.label}
          </MenuItem>
        ))}
      </Select>
      {fieldState.error && <FormHelperText>{fieldState.error.message}</FormHelperText>}
    </FormControl>
  )
}
