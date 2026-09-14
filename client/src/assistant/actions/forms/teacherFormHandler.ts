import type { Dispatch, SetStateAction } from 'react'

import type { AiAction } from '../../../services/assistantApi'
import type { TeacherFormData } from '../../../contexts/FormContext'

export function createTeacherFormHandler(
  setTeacherFormData: Dispatch<SetStateAction<TeacherFormData>>
) {
  return (action: AiAction) => {
    console.log('Teacher Form Patch Action:', action)
    const data = action.data
    console.log('Teacher Form Patch Data:', data)

    setTeacherFormData((current) => ({
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

      ...(typeof data.branch === 'string' && data.branch
        ? { branch: data.branch }
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
