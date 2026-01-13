import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

import Index from '@/views/Home.vue'
import MediaInfo from '@/views/MediaInfo.vue'
import ImageConvertor from '@/views/ImageConvertor.vue'
import JsonWebTokenInfo from '@/views/JsonWebTokenInfo.vue'
import ViewPastebin from '@/views/pastebin/ViewPastebin.vue'

export const menuList = [
  {
    title: 'Home',
    id: 'home',
    path: '',
    visable: true,
    component: Index,
  },
  {
    title: 'Media Info Explorer',
    id: 'tools_media-info',
    path: '/tools/media-info',
    visable: true,
    component: MediaInfo,
  },
  {
    title: 'Image Convertor',
    id: 'tools_image-convertor',
    path: '/tools/image-convertor',
    visable: true,
    component: ImageConvertor,
  },
  {
    title: 'ViewPastebin',
    id: 'tools_view-pastebin',
    path: '/tools/pastebin/:code',
    visable: false,
    component: ViewPastebin,
  },
  {
    title: 'JsonWebTokenInfo',
    id: 'tools_json-web-token-info',
    path: '/tools/json-web-token-info',
    visable: false,
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
