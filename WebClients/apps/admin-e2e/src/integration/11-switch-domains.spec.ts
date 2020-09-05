import { clearLocalStorage } from './_setup';

// tslint:disable: no-unused-expression
describe('switch-domains', () => {

    before(() => cy.reinitDb());

    before(() => clearLocalStorage());

    before(() => cy.login());

    it('should be active domain item', () => cy.dataCy('active-domain-item'));

    it('should be domain item', () => {
        cy.dataCy('active-domain-item').click();
        cy.dataCy('domain-item').should('have.length', 1);
        cy.dataCy('logo').click();
    });

    it('create domain', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/tenants\/\d+\/domains\/new/);
        cy.formField('name').type('new').formField('identifier').type('new').submitButton().click();
        cy.url().should('match', /\/tenants\/\d+\/domains/);
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/tenants\/\d+\/domains\/new/);
        cy.formField('name').type('new').formField('identifier').type('new').submitButton().click();
        cy.url().should('match', /\/tenants\/\d+\/domains\/new/);
        cy.cancelButton().click();
    });

    it('create env', () => {
        cy.rows(0).find('.table-actions a').eq(0).click();
        cy.url().should('match', /\/tenants\/\d+\/domains\/\d+\/new-env/);
        cy.formField('envName').type('stage').submitButton().click();
        cy.url().should('not.match', /\/tenants\/\d+\/domains\/\d+\/new-env/);
    });

    it('should be 3 domain items', () => {
        cy.dataCy('active-domain-item').click();
        cy.dataCy('domain-item').should('have.length', 3);
        cy.dataCy('logo').click();
    });

    it('click by domain item should navigate to domain', () => {
        cy.dataCy('active-domain-item').click();
        cy.dataCy('domain-item').eq(1).click();
        cy.url().should('match', /\/domains\/\d+\/app/);
    });

    it('click by tenant item should navigate to tenant', () => {
        cy.dataCy('active-domain-item').click();
        cy.dataCy('domain-item').eq(0).click();
        cy.url().should('match', /\/tenants\/\d+\/domains/);
    });
});
