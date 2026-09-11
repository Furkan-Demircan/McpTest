import type { Dispatch, SetStateAction } from 'react'

import type { AiAction } from '../../../services/assistantApi'
import type { FormData } from '../../../contexts/FormContext'

export function createStudentFormHandler(
  setFormData: Dispatch<SetStateAction<FormData>>
) {
  return (action: AiAction) => {

    console.log('Form Patch Action:', action)
    const data = action.data
    console.log('Form Patch Data:', data)


    setFormData((current) => ({
      ...current,

      ...(typeof data.firstName === 'string' && data.firstName
        ? { firstName: data.firstName }
        : {}),

      ...(typeof data.lastName === 'string' && data.lastName
        ? { lastName: data.lastName }
        : {}),

      ...(typeof data.tcNo === 'string' && data.tcNo
        ? {
            tcNo: data.tcNo.replace(/\D/g, '').slice(0, 11),
          }
        : {}),

      ...(typeof data.email === 'string' && data.email
        ? { email: data.email }
        : {}),

      ...(typeof data.motherName === 'string' && data.motherName
        ? { motherName: data.motherName }
        : {}),

      ...(typeof data.fatherName === 'string' && data.fatherName
        ? { fatherName: data.fatherName }
        : {}),

      ...(typeof data.birthDate === 'string' && data.birthDate
        ? { birthDate: data.birthDate }
        : {}),
    }))
  }
}