import ImageConvertor from '@/view/ImageConvertor.vue'
import MediaInfo from '@/view/MediaInfo.vue'
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

export const menuList = [
  {
    title: 'Media Info Explorer',
    id: 'tools_media-info',
    path: '/tools/media-info',
    component: MediaInfo,
  },
  {
    title: 'Image Convertor',
    id: 'tools_image-convertor',
    path: '/tools/image-convertor',
    component: ImageConvertor,
  },
]

const routerList: Array<RouteRecordRaw> = menuList.map((item) => ({
  name: item.id,
  path: item.path,
  component: item.component,
}))

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: routerList,
})

export default router
