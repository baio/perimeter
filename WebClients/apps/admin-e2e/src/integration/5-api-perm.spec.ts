// tslint:disable: no-unused-expression
describe('perms', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('tenant/domains');
        cy.dataCy('env-btn').click();
        cy.dataCy('apis-menu-item').click();
        cy.rows(1, 1).click();
    });

    it('app should be open', () => {
        cy.url().should('include', 'permissions');
    });

    it('create read permission', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/domains\/\d+\/apis\/\d+\/permissions\/new/);
        cy.formField('name')
            .type('read:all')
            .formField('description')
            .type('User will be able to read all items')
            .submitButton()
            .click();
        cy.url().should(
            'not.match',
            /\/domains\/\d+\/apis\/\d+\/permissions\/new/
        );
    });

    it('api should contain 1 permission', () => {
        cy.dataCy('apis-menu-item').click();
        cy.rows(0, 1).should('not.contain.text', 'read:all');
        cy.rows(1, 1).should('contain.text', 'read:all');
    });
});
