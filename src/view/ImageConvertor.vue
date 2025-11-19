<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import type { VNodeRef } from 'vue'
import {
  ColorSpace,
  ImageMagick,
  initializeImageMagick,
  MagickFormat,
  type IDefine,
} from '@imagemagick/magick-wasm'
import wasm from '@imagemagick/magick-wasm/magick.wasm?url'

const webpDesignProfile: IDefine[] = [
  {
    format: MagickFormat.WebP,
    name: 'lossless',
    value: 'false',
  },
  {
    format: MagickFormat.WebP,
    name: 'auto-filter',
    value: 'true',
  },
  {
    format: MagickFormat.WebP,
    name: 'image-hint',
    value: 'graph',
  },
  {
    format: MagickFormat.WebP,
    name: 'sharp-yuv',
    value: 'true',
  },
  {
    format: MagickFormat.WebP,
    name: 'method',
    value: '6',
  },
  {
    format: MagickFormat.WebP,
    name: 'target-size',
    value: '0',
  },
  {
    format: MagickFormat.WebP,
    name: 'preprocessing',
    value: '0',
  },
  {
    format: MagickFormat.WebP,
    name: 'target-psnr',
    value: '42',
  },
  {
    format: MagickFormat.WebP,
    name: 'segments',
    value: '3',
  },
  {
    format: MagickFormat.WebP,
    name: 'filter-sharpness',
    value: '7',
  },
  {
    format: MagickFormat.WebP,
    name: 'filter-strength',
    value: '60',
  },
]

const targetProfileListView = [
  {
    title: 'Design Assets',
    description: 'For UI/UX design images.',
    define: webpDesignProfile,
  },
  {
    title: 'Photograph(Not Impl)',
  },
  {
    title: 'Painting(Not Impl)',
  },
  {
    title: 'B&W Document(Not Impl)',
  },
  {
    title: 'Colour Document(Not Impl)',
  },
]

const pageIsProgressing = ref<boolean>(false)
const isProgressing = ref<boolean>(false)

const handleReset = () => {
  inputFileModel.value = null

  targetBlob.value = null
  targetFileImageUrlView.value = ''
  targetQualityModel.value = 75
}

const inputFileModel = ref<File | File[] | null>(null)
const inputFile = computed<File | null>(() => {
  if (!inputFileModel.value) return null

  if (Array.isArray(inputFileModel.value)) {
    const fileArray = inputFileModel.value as File[]

    if (fileArray.length <= 0) return null
    if (!fileArray[0]) return null

    return fileArray[0]
  }

  return inputFileModel.value as File
})

const inputFileImageUrlView = computed<string>(() => {
  if (!inputFile.value) return ''

  return URL.createObjectURL(inputFile.value)
})

const targetBlob = ref<Blob | null>()
const targetFileImageUrlView = ref<string>('')

const handleFileUpdated = () => {
  if (inputFileImageUrlView.value.length > 0) {
    URL.revokeObjectURL(inputFileImageUrlView.value)
  }

  if (targetFileImageUrlView.value.length > 0) {
    URL.revokeObjectURL(targetFileImageUrlView.value)
  }
}

const getArrayBufferFromUint8Array = (input: Uint8Array): ArrayBuffer => {
  if (input.buffer instanceof ArrayBuffer) {
    // 这里还需要考虑偏移和长度
    return input.buffer.slice(input.byteOffset, input.byteOffset + input.byteLength)
  } else {
    // 是 SharedArrayBuffer，需要复制到新 ArrayBuffer
    return new Uint8Array(input).buffer
  }
}

const targetQualityModel = ref<number>(75)
const handleConvertToWebp = async () => {
  isProgressing.value = true

  if (targetFileImageUrlView.value.length > 0) {
    URL.revokeObjectURL(targetFileImageUrlView.value)
  }

  const outputData = await handleConvertToTarget(
    MagickFormat.WebP,
    targetQualityModel.value,
    webpDesignProfile,
  )

  if (outputData) {
    const arrayBuffer = getArrayBufferFromUint8Array(outputData)
    targetBlob.value = new Blob([arrayBuffer], { type: 'image/webp' })
    if (targetBlob.value) {
      targetFileImageUrlView.value = URL.createObjectURL(targetBlob.value)
    }
  }

  isProgressing.value = false
}

const handleConvertToTarget = async (
  targetFormat: MagickFormat,
  quality: number,
  defines: IDefine[],
): Promise<Uint8Array | undefined> => {
  if (!inputFile.value) return

  const fileData = await inputFile.value.arrayBuffer()

  return new Promise<Uint8Array>((resolver, reject) => {
    ImageMagick.read(new Uint8Array(fileData), (image) => {
      // read finished
      image.quality = quality

      defines.forEach((define) => {
        image.settings.setDefine(define.format, define.name, define.value)
      })

      image.colorSpace = ColorSpace.sRGB
      image.write(targetFormat, (outputData) => {
        resolver(outputData)
      })
    })
  })
}

onMounted(async () => {
  pageIsProgressing.value = true
  fetch(wasm)
    .then((res) => res.arrayBuffer())
    .then(async (assembly) => {
      const validateResult = WebAssembly.validate(assembly)
      if (validateResult) {
        await initializeImageMagick(assembly)
        pageIsProgressing.value = false
      } else {
        alert('Web Assembly valid failed.')
      }
    })
})

const containerRef = ref<VNodeRef | undefined>()
</script>

<template>
  <div class="d-flex flex-fill" style="position: relative">
    <v-container class="d-flex flex-column ga-8">
      <v-overlay
        contained
        class="d-flex align-center justify-center"
        persistent
        v-model="pageIsProgressing"
      >
        <v-progress-circular indeterminate color="primary" />
      </v-overlay>

      <v-card :ref="containerRef" title="Operations">
        <v-card-text>
          <v-file-input
            accept="image/*"
            clearable
            label="Select image file."
            :loading="isProgressing"
            v-model="inputFileModel"
            @change="handleFileUpdated"
          ></v-file-input>

          Quality

          <v-slider
            v-model="targetQualityModel"
            :max="100"
            :min="50"
            :step="5"
            class="align-center"
            hide-details
          >
            <template v-slot:append>
              <v-text-field
                v-model="targetQualityModel"
                density="compact"
                style="width: 80px"
                type="number"
                hide-details
                single-line
              ></v-text-field>
            </template>
          </v-slider>

          <div class="d-flex ga-8 flex-nowrap" style="width: 100%; overflow-x:scroll">
            <div style="width: 800em;" :key="index" v-for="(profile, index) in targetProfileListView">
              <v-card :title="profile.title">
                <v-card-text>{{ profile.description }}</v-card-text>
              </v-card>
            </div>
          </div>
        </v-card-text>

        <v-card-actions>
          <v-btn :disabled="!inputFile" :loading="isProgressing" @click="handleConvertToWebp">
            Process
          </v-btn>

          <v-btn :loading="isProgressing" @click="handleReset"> RESET </v-btn>
        </v-card-actions>
      </v-card>

      <v-card title="Result">
        <v-card-text>
          <p>From {{ inputFile?.size ?? 0 / 1024 }} Kb to {{ targetBlob?.size ?? 0 / 1024 }} Kb</p>
          <p>
            {{ (Math.floor((targetBlob?.size ?? 0) / (inputFile?.size ?? Infinity)) - 1) * 100 }}%
          </p>
          <div class="d-flex flex-row">
            <!-- original -->
            <v-img :src="inputFileImageUrlView"></v-img>

            <!-- converted -->
            <v-img :src="targetFileImageUrlView"></v-img>
          </div>
        </v-card-text>
      </v-card>
    </v-container>
  </div>
</template>

<style scoped></style>
