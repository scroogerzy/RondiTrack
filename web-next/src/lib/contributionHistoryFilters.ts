import { createParser, createLoader, parseAsStringLiteral } from 'nuqs/server';
import { isValidSouthAfricanDate } from './southAfricanDateRange';

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export const memberFilterParser = createParser({
  parse: (value: string) => GUID.test(value) ? value.toLowerCase() : null,
  serialize: (value: string) => value
});

export const southAfricanDateParser = createParser({
  parse: (value: string) => isValidSouthAfricanDate(value) ? value : null,
  serialize: (value: string) => value
});

export const contributionHistoryParsers = {
  member: memberFilterParser,
  from: southAfricanDateParser,
  to: southAfricanDateParser,
  direction: parseAsStringLiteral(['asc', 'desc'] as const).withDefault('desc')
};

export const loadContributionHistoryFilters = createLoader(contributionHistoryParsers);
