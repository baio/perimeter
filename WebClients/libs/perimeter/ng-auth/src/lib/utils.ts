export const getRandomString = (l: number) => {
    const arr = new Uint32Array(l);
    window.crypto.getRandomValues(arr);
    return Array.from(arr, (dec) => '0' + dec.toString(16).substr(-2)).join('');
};

export const getSHA256 = (plain: string) => {
    const encoder = new TextEncoder();
    const data = encoder.encode(plain);
    return window.crypto.subtle.digest('SHA-256', data);
};

export const base64arrayEncode = (arr: ArrayBuffer) =>
    btoa(String.fromCharCode.apply(null, new Uint8Array(arr)))
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');

export const pkceChallengeFormVerifier = async (str: string) => {
    const hashed = await getSHA256(str);
    return base64arrayEncode(hashed);
};
