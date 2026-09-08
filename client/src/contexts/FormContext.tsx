import {
  createContext,
  type Dispatch,
  type SetStateAction,
} from 'react'

export interface FormData {
  ad: string
  soyad: string
  tcNo: string
  email: string
  anneAdi: string
  babaAdi: string
  dogumTarihi: string
}

export interface FormContextType {
  formData: FormData
  setFormData: Dispatch<SetStateAction<FormData>>
}

export const initialFormData: FormData = {
  ad: '',
  soyad: '',
  tcNo: '',
  email: '',
  anneAdi: '',
  babaAdi: '',
  dogumTarihi: '',
}

export const FormContext =
  createContext<FormContextType | undefined>(undefined)