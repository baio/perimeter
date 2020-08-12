import { ACCESS_TOKEN } from './_setup';

describe('domains', () => {
    before(() => {
        cy.visit('domains/pool');
        cy.window().then((win) =>
            win.sessionStorage.setItem('access_token', ACCESS_TOKEN)
        );
    });

    it('create domain', () => {
        cy.dataCy('create-item').click();
        cy.url().should('include', '/domains/pool/new');
    });
});
