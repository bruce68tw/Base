import _Ajax from "./_Ajax";
export default class _Locale {
    static async setLocaleA(code) {
        await _Ajax.getStrA('../Fun/SetLocale', { code: code }, function (msg) {
            location.reload();
        });
        const module = await import(`../Locale/${code}/_BR`);
        window._BR = module.default;
    }
}
