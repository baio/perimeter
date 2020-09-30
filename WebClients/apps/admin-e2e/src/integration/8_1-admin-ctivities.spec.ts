import { clearLocalStorage } from './_setup';

// tslint:disable: no-unused-expression
describe('admins', () => {
    before(() => cy.reinitDb());

    before(() => clearLocalStorage());

    before(() => {
        cy.login();
    });

    before(() => {
        cy.dataCy('env-btn').click();
        cy.dataCy('admin-activities-menu-item').click();
    });

    it('admin activities must have 1 active admin', () => {
        cy.url().should('include', 'admin-activities');
        cy.rows().should('have.length', 1);
    });
});
