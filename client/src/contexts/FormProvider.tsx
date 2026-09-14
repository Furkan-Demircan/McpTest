import React, { useState, type ReactNode } from 'react'
import {
  FormContext,
  initialStudentFormData,
  initialTeacherFormData,
  type FormData,
  type TeacherFormData,
} from './FormContext'

interface FormProviderProps {
  children: ReactNode
}

export const FormProvider: React.FC<FormProviderProps> = ({
  children,
}) => {
  const [studentFormData, setStudentFormData] =
    useState<FormData>(initialStudentFormData)
  const [teacherFormData, setTeacherFormData] =
    useState<TeacherFormData>(initialTeacherFormData)

  return (
    <FormContext.Provider
      value={{
        formData: studentFormData,
        setFormData: setStudentFormData,
        studentFormData,
        setStudentFormData,
        teacherFormData,
        setTeacherFormData,
      }}
    >
      {children}
    </FormContext.Provider>
  )
}