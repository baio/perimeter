export const EMPTY = { empty: true };
export const MISS_UPPER_CASE_LETTER = { ['miss-upper-case-letter']: true };
export const MISS_LOWER_CASE_LETTER = { ['miss-lower-case-letter']: true };
export const MISS_DIGIT = { ['miss-digit']: true };
export const MISS_SPECIAL_CHAR = { ['miss-special-char']: true };
export const EMPTY_STRING = { required: true };
export const MIN_LENGTH = (x: number) => ({
    minlength: x,
});
export const MAX_LENGTH = (x: number) => ({
    maxlength: x,
});
export const MESSAGE = (x: string) => ({
    message: x,
});
