// 43 - 128
export const getRandomString = (length: number) => {
    let result = '';
    const characters =
        'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    const charactersLength = characters.length;
    for (let i = 0; i < length; i++) {
        result += characters.charAt(
            Math.floor(Math.random() * charactersLength)
        );
    }
    return result;
};

export const getSHA256 = (plain: string) => {
    const encoder = new TextEncoder();
    const data = encoder.encode(plain);
    return window.crypto.subtle.digest('SHA-256', data);
};

export const base64arrayEncode = (arr: ArrayBuffer) =>
    // TODO !
    btoa(String.fromCharCode.apply(null, new Uint8Array(arr)))/*
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');*/

export const pkceChallengeFormVerifier = async (str: string) => {
    const hashed = await getSHA256(str);
    return base64arrayEncode(hashed);
};
