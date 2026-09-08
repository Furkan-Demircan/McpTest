import React, { useState, type ReactNode } from 'react'
import {
  FormContext,
  initialFormData,
} from './FormContext'

interface FormProviderProps {
  children: ReactNode
}

export const FormProvider: React.FC<FormProviderProps> = ({
  children,
}) => {
  const [formData, setFormData] =
    useState(initialFormData)

  return (
    <FormContext.Provider
      value={{
        formData,
        setFormData,
      }}
    >
      {children}
    </FormContext.Provider>
  )
}