import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { getMe, getLoginUrl, importCharacters, getNeedsRollup } from '../api';

const API_URL = 'http://localhost:5000';

const server = setupServer();

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('getMe', () => {
  it('returns the user on 200', async () => {
    server.use(
      http.get(`${API_URL}/api/auth/me`, () =>
        HttpResponse.json({ id: '1', battleTag: 'Test#1234', isAuthenticated: true })
      )
    );
    const user = await getMe();
    expect(user).toEqual({ id: '1', battleTag: 'Test#1234', isAuthenticated: true });
  });

  it('returns null on 401 (not authenticated)', async () => {
    server.use(
      http.get(`${API_URL}/api/auth/me`, () =>
        new HttpResponse(null, { status: 401 })
      )
    );
    const user = await getMe();
    expect(user).toBeNull();
  });

  it('returns null on network error', async () => {
    server.use(
      http.get(`${API_URL}/api/auth/me`, () => HttpResponse.error())
    );
    const user = await getMe();
    expect(user).toBeNull();
  });
});

describe('getLoginUrl', () => {
  it('returns a URL pointing to the backend auth/login endpoint', () => {
    const url = getLoginUrl();
    expect(url).toBe(`${API_URL}/api/auth/login`);
  });
});

describe('importCharacters', () => {
  it('returns imported count on success', async () => {
    server.use(
      http.post(`${API_URL}/api/characters/import`, () =>
        HttpResponse.json({ imported: 3, total: 5 })
      )
    );
    const result = await importCharacters('US');
    expect(result.imported).toBe(3);
    expect(result.apiFailed).toBeUndefined();
  });

  it('passes apiFailed: true through when Classic API is unavailable', async () => {
    server.use(
      http.post(`${API_URL}/api/characters/import`, () =>
        HttpResponse.json({ imported: 0, total: 0, apiFailed: true })
      )
    );
    const result = await importCharacters('US');
    expect(result.apiFailed).toBe(true);
  });
});

describe('getNeedsRollup', () => {
  it('returns array of gear needs on success', async () => {
    const mockNeeds = [
      {
        characterId: 1,
        characterName: 'Testchar',
        characterClass: 'Warrior',
        slotName: 'Head',
        currentItem: 'Blue Helm',
        bisItem: 'Tier 6 Helm',
        bisSource: 'Black Temple',
      },
    ];
    server.use(
      http.get(`${API_URL}/api/gear/needs`, () => HttpResponse.json(mockNeeds))
    );
    const needs = await getNeedsRollup();
    expect(needs).toHaveLength(1);
    expect(needs[0].characterName).toBe('Testchar');
  });

  it('throws on non-2xx status', async () => {
    server.use(
      http.get(`${API_URL}/api/gear/needs`, () =>
        new HttpResponse('Internal Server Error', { status: 500 })
      )
    );
    await expect(getNeedsRollup()).rejects.toThrow('API 500');
  });
});
