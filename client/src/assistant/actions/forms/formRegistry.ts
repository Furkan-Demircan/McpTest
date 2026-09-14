import type { Dispatch, SetStateAction } from 'react'
import type {
    FormData,
    TeacherFormData,
} from '../../../contexts/FormContext'
import { createStudentFormHandler } from '../../actions/forms/studentFormHandler'
import { createTeacherFormHandler } from '../../actions/forms/teacherFormHandler'
import type { AiActionHandler } from '../types'

export type SupportedFormData =
    | FormData
    | TeacherFormData

interface FormRegistryItem {
    type: 'student' | 'teacher'
    paths: string[]
    getFormData: () => SupportedFormData
    getFormHandler: () => AiActionHandler
}

interface CreateFormRegistryOptions {
    studentFormData: FormData
    teacherFormData: TeacherFormData

    setStudentFormData: Dispatch<SetStateAction<FormData>>
    setTeacherFormData: Dispatch<SetStateAction<TeacherFormData>>
}

export function createFormRegistry({
    studentFormData,
    teacherFormData,
    setStudentFormData,
    setTeacherFormData,
}: CreateFormRegistryOptions) {
    const studentFormHandler = createStudentFormHandler(setStudentFormData)
    const teacherFormHandler = createTeacherFormHandler(setTeacherFormData)

    const forms: FormRegistryItem[] = [
    {
    paths: [
        '/form',
        '/ogrenci',
        '/ogrenci-ekle',
        '/student',
    ],
    type: 'student',
    getFormData: () => studentFormData,
    getFormHandler: () => studentFormHandler,
    },
    {
    paths: [
    '/teacher',
    '/teacher-form',
    '/ogretmen',
    '/ogretmen-ekle',
    ],
    type: 'teacher',
    getFormData: () => teacherFormData,
    getFormHandler: () => teacherFormHandler,
    },
    ]

    return {
        getFormData(pathname: string): SupportedFormData | null {
            const form = forms.find((item) =>
                item.paths.includes(pathname)
            )

            return form?.getFormData() ?? null
        },  
        
        getFormType(pathname: string) {
            const form = forms.find((item) =>
            item.paths.includes(pathname)
        )

        return form?.type ?? null
        },
        getFormHandler(pathname: string) {
            const form = forms.find((item) =>
                item.paths.includes(pathname)
            )

            return form?.getFormHandler() ?? null
        }
}
}