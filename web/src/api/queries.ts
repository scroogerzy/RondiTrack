import {
  useInfiniteQuery,
  useQuery,
} from '@tanstack/react-query'
import { api, unwrap } from './client'
import type { components } from './schema'
import { ApiError } from '../lib/errors'

export const queryKeys = {
  me: ['me'] as const,
  home: ['me', 'home'] as const,
  stokvels: ['stokvels'] as const,
  stokvel: (stokvelId: string) => ['stokvels', stokvelId] as const,
  cycles: (stokvelId: string) => ['stokvels', stokvelId, 'cycles'] as const,
  contributions: (stokvelId: string, cycleId: string) =>
    ['stokvels', stokvelId, 'cycles', cycleId, 'contributions'] as const,
}

export type HomeResponse = components['schemas']['HomeResponse']
export type StokvelHomeItem = components['schemas']['StokvelHomeItem']
export type Cycle = components['schemas']['ContributionCycleResponse']
export type Contribution = components['schemas']['ContributionResponse']

function retryPolicy(failureCount: number, error: Error): boolean {
  if (error instanceof ApiError && error.status >= 400 && error.status < 500) {
    return false
  }

  return failureCount < 2
}

export function useHome() {
  return useQuery({
    queryKey: queryKeys.home,
    queryFn: async () => unwrap(await api.GET('/api/v1/me/home')),
    retry: retryPolicy,
  })
}

export function useMe() {
  return useHome()
}

export function useStokvels() {
  return useQuery({
    queryKey: queryKeys.stokvels,
    queryFn: async () => {
      const home = await unwrap(await api.GET('/api/v1/me/home'))
      return home.stokvels
    },
    retry: retryPolicy,
  })
}

export function useStokvel(stokvelId: string) {
  return useQuery({
    queryKey: queryKeys.stokvel(stokvelId),
    enabled: Boolean(stokvelId),
    queryFn: async () => {
      const home = await unwrap(await api.GET('/api/v1/me/home'))
      const stokvel = home.stokvels.find((item) => item.stokvelId === stokvelId)

      if (!stokvel) {
        throw new ApiError(404, 'Stokvel not found')
      }

      return stokvel
    },
    retry: retryPolicy,
  })
}

export function useCycles(stokvelId: string) {
  return useInfiniteQuery({
    queryKey: queryKeys.cycles(stokvelId),
    enabled: Boolean(stokvelId),
    initialPageParam: '',
    queryFn: async ({ pageParam }) => unwrap(await api.GET(
      '/api/v1/stokvels/{stokvelId}/cycles',
      {
        params: {
          path: { stokvelId },
          query: {
            pageSize: 20,
            sortBy: 'periodNumber',
            sortDirection: 'desc',
            ...(pageParam ? { pageToken: pageParam } : {}),
          },
        },
      },
    )),
    getNextPageParam: (lastPage) =>
      lastPage.nextPageToken ? lastPage.nextPageToken : undefined,
    retry: retryPolicy,
  })
}


export function useStokvelMembers(stokvelId: string, enabled = true) {
  return useQuery({
    queryKey: ['stokvels', stokvelId, 'members'] as const,
    enabled: Boolean(stokvelId) && enabled,
    queryFn: async () => unwrap(await api.GET(
      '/api/v1/stokvels/{stokvelId}/members',
      { params: { path: { stokvelId } } },
    )),
    retry: retryPolicy,
  })
}

export function useContributions(stokvelId: string, cycleId: string) {
  return useInfiniteQuery({
    queryKey: queryKeys.contributions(stokvelId, cycleId),
    enabled: Boolean(stokvelId && cycleId),
    initialPageParam: '',
    queryFn: async ({ pageParam }) => unwrap(await api.GET(
      '/api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions',
      {
        params: {
          path: { stokvelId, cycleId },
          query: {
            pageSize: 20,
            sortBy: 'recordedAt',
            sortDirection: 'desc',
            ...(pageParam ? { pageToken: pageParam } : {}),
          },
        },
      },
    )),
    getNextPageParam: (lastPage) =>
      lastPage.nextPageToken ? lastPage.nextPageToken : undefined,
    retry: retryPolicy,
  })
}
