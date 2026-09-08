import { useContext } from 'react'
import {
  FormContext,
  type FormContextType,
} from './FormContext'

export const useFormContext = (): FormContextType => {
  const context = useContext(FormContext)

  if (!context) {
    throw new Error(
      'useFormContext must be used within a FormProvider'
    )
  }

  return context
}