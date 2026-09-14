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

export type StudentFormData = FormData

export interface TeacherFormData {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  branch: string
  motherName: string
  fatherName: string
  birthDate: string
}

export interface FormContextType {
  // Geriye dönük uyumluluk (Öğrenci formuna işaret eder)
  formData: FormData
  setFormData: Dispatch<SetStateAction<FormData>>
  
  // Öğrenci formu verisi
  studentFormData: FormData
  setStudentFormData: Dispatch<SetStateAction<FormData>>

  // Öğretmen formu verisi
  teacherFormData: TeacherFormData
  setTeacherFormData: Dispatch<SetStateAction<TeacherFormData>>
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

export const initialStudentFormData: FormData = initialFormData

export const initialTeacherFormData: TeacherFormData = {
  firstName: '',
  lastName: '',
  tcNo: '',
  email: '',
  branch: '',
  motherName: '',
  fatherName: '',
  birthDate: '',
}

export const FormContext =
  createContext<FormContextType | undefined>(undefined)