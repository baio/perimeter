export const EMPTY = { empty: true };
export const MISS_UPPER_CASE_LETTER = { ['miss-upper-case-letter']: true };
export const NOT_DOMAIN_NAME = { ['not-domain-name']: true };
export const MISS_LOWER_CASE_LETTER = { ['miss-lower-case-letter']: true };
export const MISS_DIGIT = { ['miss-digit']: true };
export const MISS_SPECIAL_CHAR = { ['miss-special-char']: true };
export const UNIQUE_CONFLICT = { unique_conflict: true };
export const EMPTY_STRING = { required: true };
export const PASSWORD = { password: true };
export const MIN_LENGTH = (x: number) => ({
    minlength: x,
});
export const MAX_LENGTH = (x: number) => ({
    maxlength: x,
});
export const MESSAGE = (x: string) => ({
    message: x,
});
