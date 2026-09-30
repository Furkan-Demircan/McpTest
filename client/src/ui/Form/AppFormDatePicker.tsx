import { DatePicker, type DatePickerProps } from '@mui/x-date-pickers/DatePicker'
import { format, isValid } from 'date-fns'
import { useController, type Control, type FieldPath, type FieldValues } from 'react-hook-form'
import { useAiField } from '../ai/useAiField'

export interface AppFormDatePickerProps<TFieldValues extends FieldValues>
  extends Omit<DatePickerProps, 'name' | 'value' | 'onChange' | 'label'> {
  name: FieldPath<TFieldValues>
  control: Control<TFieldValues>
  label?: string
  required?: boolean
}

const toIso = (date: Date) => format(date, 'yyyy-MM-dd')

export const AppFormDatePicker = <TFieldValues extends FieldValues>({
  name,
  control,
  label,
  required,
  slotProps,
  ...datePickerProps
}: AppFormDatePickerProps<TFieldValues>) => {
  const { field, fieldState } = useController({ name, control })
  const { minDate, maxDate } = datePickerProps

  // Asistan: YYYY-MM-DD alır, Date'e çevirip bileşenin sınırlarına (minDate/maxDate) göre kontrol eder
  const aiProps = useAiField(name, {
    label: label ?? name,
    kind: 'input',
    required,
    min: minDate ? toIso(minDate as Date) : undefined,
    max: maxDate ? toIso(maxDate as Date) : undefined,
    getValue: () => {
      const value = field.value as Date | null | undefined
      return value && isValid(value) ? toIso(value) : undefined
    },
    write: (value) => {
      // Boş değer tarihi temizler
      if (value == null || String(value).trim() === '') {
        field.onChange(null)
        return { ok: true }
      }

      const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(String(value ?? ''))
      if (!match) {
        return { ok: false, reason: 'Tarih YYYY-MM-DD biçiminde olmalı.' }
      }

      const date = new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]))
      if (
        (minDate && date < (minDate as Date)) ||
        (maxDate && date > (maxDate as Date))
      ) {
        const range = `${minDate ? toIso(minDate as Date) : '…'} – ${maxDate ? toIso(maxDate as Date) : '…'}`
        return { ok: false, reason: `Tarih izin verilen aralıkta değil (${range}).` }
      }

      field.onChange(date)
      return { ok: true }
    },
  })

  return (
    <DatePicker
      {...datePickerProps}
      name={field.name}
      inputRef={field.ref}
      value={(field.value as Date | null | undefined) ?? null}
      onChange={(date) => field.onChange(date)}
      label={required ? `${label} *` : label}
      slotProps={{
        ...slotProps,
        textField: {
          ...aiProps,
          fullWidth: true,
          size: 'small',
          onBlur: field.onBlur,
          error: !!fieldState.error,
          helperText: fieldState.error?.message,
          ...slotProps?.textField,
        },
      }}
    />
  )
}
