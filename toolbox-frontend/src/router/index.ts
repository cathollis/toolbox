import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

import Index from '@/views/Home.vue'
import MediaInfo from '@/views/MediaInfo.vue'
import ImageConvertor from '@/views/ImageConvertor.vue'
import JsonWebTokenInfo from '@/views/JsonWebTokenInfo.vue'

export const menuList = [
  {
    title: 'Home',
    id: 'home',
    path: '',
    component: Index,
  },
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
  // {
  //   title: 'Pastebin',
  //   id: 'tools_pastebin',
  //   path: '/tools/pastebin/:code',
  //   component: PasteBin,
  // },
  {
    title: 'JsonWebTokenInfo',
    id: 'tools_json-web-token-info',
    path: '/tools/json-web-token-info',
    component: JsonWebTokenInfo,
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
