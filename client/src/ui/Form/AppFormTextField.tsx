import { TextField, type TextFieldProps } from '@mui/material'
import { useController, type Control, type FieldPath, type FieldValues } from 'react-hook-form'
import { useAiField } from '../ai/useAiField'

export interface AppFormTextFieldProps<TFieldValues extends FieldValues>
  extends Omit<TextFieldProps, 'name' | 'value' | 'onChange' | 'error'> {
  name: FieldPath<TFieldValues>
  control: Control<TFieldValues>
  /** Girilen değeri form state'e yazmadan önce dönüştürür (örn. sadece rakam) */
  transform?: (value: string) => string
}

export const AppFormTextField = <TFieldValues extends FieldValues>({
  name,
  control,
  transform,
  helperText,
  ...textFieldProps
}: AppFormTextFieldProps<TFieldValues>) => {
  const { field, fieldState } = useController({ name, control })
  const toValue = (text: string) => (transform ? transform(text) : text)

  // Asistan: aynı transform'dan geçerek react-hook-form'a yazar
  const aiProps = useAiField(name, {
    label: String(textFieldProps.label ?? name),
    kind: 'input',
    required: textFieldProps.required,
    getValue: () => (field.value ? String(field.value) : undefined),
    write: (value) => {
      field.onChange(toValue(String(value ?? '')))
      return { ok: true }
    },
  })

  return (
    <TextField
      {...textFieldProps}
      {...aiProps}
      name={field.name}
      inputRef={field.ref}
      onBlur={field.onBlur}
      value={field.value ?? ''}
      onChange={(event) => field.onChange(toValue(event.target.value))}
      error={!!fieldState.error}
      helperText={fieldState.error?.message ?? helperText}
      fullWidth
      size="small"
    />
  )
}
