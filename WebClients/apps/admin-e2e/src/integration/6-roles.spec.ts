// tslint:disable: no-unused-expression
describe('roles', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('tenant/domains');
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
        cy.dataCy('create-item')
            .click()
            .formField('name')
            .type('write:all')
            .formField('description')
            .type('User will be able to write all items')
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

        describe('create', () => {
            before(() => {
                cy.dataCy('create-item').click();
            });
            it('form should be open', () => {
                cy.url().should('match', /\/domains\/\d+\/roles\/new/);
            });
            it('create', () => {
                cy.formField('name')
                    .type('developer')
                    .formField('description')
                    .type('developer')
                    .formSelectChoose('permissionIds', 0)
                    .submitButton()
                    .click();
                cy.url().should('not.match', /\/domains\/\d+\/roles\/new/);
                cy.rows().should('have.length', 1);
                cy.rows(0, 2).should('contain.text', 'write:all');
            });
        });

        describe('edit', () => {
            it('load app edit form data', () => {
                cy.rows(0).click();
                cy.url().should('match', /\/domains\/\d+\/roles\/\d+/);
                cy.formField('name').should((input) => {
                    const val = input.val();
                    expect(val).be.not.empty;
                });
            });

            it('edit form data', () => {
                cy.formField('name')
                    .formSelectChoose('permissionIds', 1)
                    .submitButton()
                    .click();
                cy.url().should('not.match', /\/domains\/\d+\/roles\/\d+/);
                cy.rows().should('have.length', 1);
                cy.rows(0, 2).should('contain.text', 'read:all');
            });
        });

        describe.skip('delete', () => {
            it('remove', () => {
                cy.rowCommand(0, 0)
                    .click()
                    .confirmYesButton()
                    .click()
                    .rows()
                    .should('have.length', 0);
            });
        });
    });
});
