import { clearLocalStorage } from './_setup';

// tslint:disable: no-unused-expression
describe('login', () => {
    before(() => cy.reinitDb());

    before(() => clearLocalStorage());

    before(() => cy.visit('/'));

    it('unauthorized user must be redirected to home page', () => {
        cy.url().should('include', 'home');
    });

    describe('when login', () => {
        before(() => cy.login());

        it('must be redirected to first available tenant', () => {
            cy.url().should('match', /\/tenants\/\d+\/domains/);
        });

        describe('when move to domain', () => {
            before(() => cy.dataCy('env-btn').click());

            it('must be redirected to domain', () => {
                cy.url().should('match', /\/domains\/\d+\/info/);
            });

            describe('when navigate back to tenant', () => {
                before(() => cy.visit('/'));

                it('must be redirected to tenant', () => {
                    cy.url().should('match', /\/tenants\/\d+\/domains/);
                });
            });
        });
    });
});
