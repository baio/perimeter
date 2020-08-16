// tslint:disable: no-unused-expression
describe('perms', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
        cy.dataCy('env-btn').click();
        cy.dataCy('apis-menu-item').click();
        cy.rows(1, 1).click();
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

    describe('roles', () => {
        it('open roles', () => {
            cy.dataCy('roles-menu-item').click();
        });

        it('roles should be open', () => {
            cy.url().should('include', 'roles');
        });
    });
});
