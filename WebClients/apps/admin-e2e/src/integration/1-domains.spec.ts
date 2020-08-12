import { ACCESS_TOKEN } from './_setup';

describe('domains', () => {
    before(() => {
        cy.visit('domains/pool');
        cy.window().then((win) =>
            win.sessionStorage.setItem('access_token', ACCESS_TOKEN)
        );
    });

    describe('create / edit', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('include', '/domains/pool/new');
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('not.include', '/domains/pool/new');
        });
    });

    /*
    describe('list of items', () => {
        it('default list count', () => {
            cy.get('table').find('tr').should('have.length', 2);
        });
    });
    */
});
