import {
  createContext,
  type Dispatch,
  type SetStateAction,
} from 'react'

export interface FormData {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  motherName: string
  fatherName: string
  birthDate: string
}

export interface FormContextType {
  formData: FormData
  setFormData: Dispatch<SetStateAction<FormData>>
}

export const initialFormData: FormData = {
  firstName: '',
  lastName: '',
  tcNo: '',
  email: '',
  motherName: '',
  fatherName: '',
  birthDate: '',
}

export const FormContext =
  createContext<FormContextType | undefined>(undefined)