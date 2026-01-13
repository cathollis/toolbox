import { msalInstance } from '@/utils/msal'
import axios from 'axios'

const microsoftApi = axios.create({
  baseURL: 'https://graph.microsoft.com/v1.0/',
  timeout: 6000,
})

microsoftApi.interceptors.request.use(
  (config) => {
    const token = msalInstance.getTokenCache()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    console.log(error)

    Promise.reject(error)
  },
)

export default microsoftApi
