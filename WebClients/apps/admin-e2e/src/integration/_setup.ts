export const EMAIL = 'hahijo5833@acceptmail.net';
export const PASSWORD = '#6VvR&^';

// https://github.com/cypress-io/cypress/issues/461
export const clearLocalStorage = () => {
    Cypress.LocalStorage.clear();
    cy.window().then((win) => {
        win.localStorage.clear();
    });
};
Cypress.LocalStorage.clear = function () {};
