// tslint:disable: no-unused-expression
describe('domains', () => {
    const UPDATED_NAME = 'updated domain';

    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
    });

    describe('edit', () => {
        it('load domain edit form data', () => {
            cy.rows().first().click();
            cy.url().should('match', /\/domains\/pool\/\d+/);
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
            cy.url().should('not.match', /\/domains\/pool\/\d+/);
            cy.rows().should('have.length', 1);
        });
    });

    describe('create', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('include', '/domains/pool/new');
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('not.include', '/domains/pool/new');
        });

        it('create domain with same name should fail', () => {
            cy.dataCy('create-item').click();
            cy.url().should('include', '/domains/pool/new');
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('include', '/domains/pool/new');
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

    describe.only('add env', () => {
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
        });
    });
});
