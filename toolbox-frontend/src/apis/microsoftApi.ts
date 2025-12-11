import { useUser } from '@/stores/UserStore'
import axios from 'axios'

const microsoftApi = axios.create({
  baseURL: 'https://graph.microsoft.com/v1.0/',
  timeout: 6000,
})

microsoftApi.interceptors.request.use(
  (config) => {
    const userStore = useUser()
    const token = userStore.token
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
