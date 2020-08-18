// tslint:disable: no-unused-expression
describe('user', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
        cy.dataCy('env-btn').click();
        cy.dataCy('apis-menu-item').click();
        cy.rows(1, 1).click();
        cy.dataCy('create-item').click();
        cy.formField('name')
            .type('read:all')
            .formField('description')
            .type('User will be able to read all items')
            .submitButton()
            .click();
        cy.dataCy('create-item').click();
        cy.formField('name')
            .type('write:all')
            .formField('description')
            .type('User will be able to write all items')
            .submitButton()
            .click();
        cy.dataCy('roles-menu-item').click();
        cy.dataCy('create-item').click();
        cy.formField('name')
            .type('admin')
            .formField('description')
            .type('developer')
            .formSelectChoose('permissionIds', 0)
            .submitButton()
            .click();
        cy.dataCy('create-item').click();
        cy.formField('name')
            .type('developer')
            .formField('description')
            .type('developer')
            .formSelectChoose('permissionIds', 1)
            .submitButton()
            .click();
        cy.dataCy('users-menu-item').click();
    });

    it('roles should be open', () => {
        cy.url().should('include', 'users');
        cy.rows().should('have.length', 0);
    });

    it('create', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/domains\/\d+\/users\/new/);

        cy.formField('userEmail')
            .type('dev@test.dev')
            .formSelectChoose('rolesIds', 0)
            .submitButton()
            .click();

        cy.rows().should('have.length', 1);
        cy.rows(0, 1).should('contain.text', 'admin');
    });

    it('edit', () => {
        cy.rows(0).click();
        cy.url().should('match', /\/domains\/\d+\/users\/\w+/);

        cy.formField('userEmail')
            .formSelectChoose('rolesIds', 1)
            .submitButton()
            .click();

        cy.rows().should('have.length', 1);
        cy.rows(0, 1).should('contain.text', 'developer');
        cy.rows(0, 1).should('contain.text', 'admin');
    });

    it.skip('remove', () => {
        cy.rowCommand(0, 0)
            .click()
            .confirmYesButton()
            .click()
            .rows()
            .should('have.length', 0);
    });
});
