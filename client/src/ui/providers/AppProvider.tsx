import { createTheme, ThemeProvider, useMediaQuery } from '@mui/material'
import { LocalizationProvider } from '@mui/x-date-pickers'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useMemo, type ReactNode } from 'react'

/** CRM'deki packages/ui AppProvider'ının karşılığı: MUI teması + tarih yerelleştirmesi (tr). */
export function AppProvider({ children }: { children: ReactNode }) {
  const prefersDark = useMediaQuery('(prefers-color-scheme: dark)')
  const theme = useMemo(
    () => createTheme({ palette: { mode: prefersDark ? 'dark' : 'light', primary: { main: '#aa3bff' } } }),
    [prefersDark]
  )

  return (
    <ThemeProvider theme={theme}>
      <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
        {children}
      </LocalizationProvider>
    </ThemeProvider>
  )
}
