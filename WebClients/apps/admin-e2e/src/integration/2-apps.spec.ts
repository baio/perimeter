// tslint:disable: no-unused-expression
describe('apps', () => {
    const UPDATED_NAME = 'updated name';
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
        cy.dataCy('env-btn').click();
    });

    it('app should be open', () => {
        cy.url().should('include', 'apps');
    });

    describe('edit', () => {
        it('load app edit form data', () => {
            cy.rows(1).click();
            cy.url().should('match', /\/domains\/\d+\/apps\/\d+/);
            cy.formField('name').should((input) => {
                const val = input.val();
                expect(val).be.not.empty;
            });
        });

        it('edit form data', () => {
            cy.formField('name')
                .clear()
                .type(UPDATED_NAME)
                .submitButton()
                .click();
            cy.url().should('not.match', /\/domains\/\d+\/apps\/\d+/);
            cy.rows().should('have.length', 1);
        });
    });

    describe.only('create', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('match', /\/domains\/\d+\/apps\/new/);
            cy.formField('name')
                .type('new')
                .formField('idTokenExpiresIn')
                .type('15')
                .formField('refreshTokenExpiresIn')
                .type('500')
                .submitButton()
                .click();
            cy.url().should('not.match', /\/domains\/\d+\/apps\/new/);
        });

        it('create app with same name should fail', () => {
            cy.dataCy('create-item').click();
            cy.url().should('match', /\/domains\/\d+\/apps\/new/);
            cy.formField('name')
                .type('new')
                .formField('idTokenExpiresIn')
                .type('15')
                .formField('refreshTokenExpiresIn')
                .type('500')
                .submitButton()
                .click();
            cy.url().should('match', /\/domains\/\d+\/apps\/new/);
            cy.cancelButton().click();
        });
    });

    describe('list', () => {
        it('rows count 2 : sample + created', () => {
            cy.rows().should('have.length', 2);
        });

        it('latest item on top', () => {
            cy.rows(0).get('td').first().should('contain.text', 'new');
        });

        it('sort by created change rows positions', () => {
            cy.get('table thead th').eq(2).click().click();
            cy.rows(0, 0).should('contain.text', UPDATED_NAME);
            cy.rows(1, 0).should('contain.text', 'new');
        });

        it('sort by name change rows positions', () => {
            cy.get('table thead th').eq(0).click();
            cy.rows(0, 0).should('contain.text', 'new');
            cy.rows(1, 0).should('contain.text', UPDATED_NAME);
            cy.get('table thead th').eq(0).click();
            cy.rows(0, 0).should('contain.text', UPDATED_NAME);
            cy.rows(1, 0).should('contain.text', 'new');
        });

        it('filter by updated should give 1 item', () => {
            cy.dataCy('text-search').type('domain').type('{enter}');
            cy.rows().should('have.length', 1);
            cy.rows(0, 0).should('contain.text', UPDATED_NAME);
        });
    });

    describe('add env', () => {
        it('add env', () => {
            cy.rows(0).find('.table-actions a').eq(0).click();
            cy.url().should('match', /\/domains\/pool\/\d+\/new-env/);
        });

        it('create', () => {
            cy.formField('envName').type('stage').submitButton().click();
            cy.url().should('not.match', /\/domains\/pool\/\d+\/new-env/);
        });

        it('add env with the same name should give error', () => {
            cy.rows(0).find('.table-actions a').eq(0).click();
            cy.formField('envName').type('stage').submitButton().click();
            cy.url().should('match', /\/domains\/pool\/\d+\/new-env/);
            cy.cancelButton().click();
        });
    });

    describe('delete', () => {
        it('remove domain', () => {
            cy.rows(0).find('.table-actions a').eq(1).click();
            cy.confirmYesButton().click();
            cy.rows().should('have.length', 0);
        });

        it('after reset filter there should be 1 rows', () => {
            cy.dataCy('text-search').clear().type('{enter}');
            cy.rows().should('have.length', 1);
        });
    });
});
