/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 by Bart Kiers
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
 * associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute,
 * sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
 * NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Project : sqlite-parser; an ANTLR4 grammar for SQLite https://github.com/bkiers/sqlite-parser
 * Developed by:
 *     Bart Kiers, bart@big-o.nl
 *     Martin Mirchev, marti_2203@abv.bg
 *     Mike Lische, mike@lischke-online.de
 */

// $antlr-format alignTrailingComments on, columnLimit 130, minEmptyLines 1, maxEmptyLinesToKeep 1, reflowComments off
// $antlr-format useTab off, allowShortRulesOnASingleLine off, allowShortBlocksOnASingleLine on, alignSemicolons ownLine

parser grammar SQLiteParser;

options {
    tokenVocab = SQLiteLexer;
}

parse: (sql_stmt_list)* EOF
;

sql_stmt_list:
    SCOL* sql_stmt (SCOL+ sql_stmt)* SCOL*
;

sql_stmt: (EXPLAIN (QUERY PLAN)?)? (
        alter_table_stmt
        | analyze_stmt
        | attach_stmt
        | begin_stmt
        | commit_stmt
        | create_index_stmt
        | create_table_stmt
        | create_trigger_stmt
        | create_view_stmt
        | create_virtual_table_stmt
        | delete_stmt
        | delete_stmt_limited
        | detach_stmt
        | drop_stmt
        | insert_stmt
        | pragma_stmt
        | reindex_stmt
        | release_stmt
        | rollback_stmt
        | savepoint_stmt
        | select_stmt
        | update_stmt
        | update_stmt_limited
        | vacuum_stmt
    )
;

alter_table_stmt:
    ALTER TABLE (schema_name DOT)? table_name (
        RENAME (
            TO new_table_name = table_name
            | COLUMN? old_column_name = column_name TO new_column_name = column_name
        )
        | ADD COLUMN? column_def
        | DROP COLUMN? column_name
    )
;

analyze_stmt:
    ANALYZE (schema_name | (schema_name DOT)? table_or_index_name)?
;

attach_stmt:
    ATTACH DATABASE? expr AS schema_name
;

begin_stmt:
    BEGIN (DEFERRED | IMMEDIATE | EXCLUSIVE)? (
        TRANSACTION transaction_name?
    )?
;

commit_stmt: (COMMIT | END) TRANSACTION?
;

rollback_stmt:
    ROLLBACK TRANSACTION? (TO SAVEPOINT? savepoint_name)?
;

savepoint_stmt:
    SAVEPOINT savepoint_name
;

release_stmt:
    RELEASE SAVEPOINT? savepoint_name
;

create_index_stmt:
    CREATE UNIQUE? INDEX (IF NOT EXISTS)? (schema_name DOT)? index_name ON table_name OPEN_PAR
        indexed_column (COMMA indexed_column)* CLOSE_PAR (WHERE expr)?
;

indexed_column: (column_name | expr) (COLLATE collation_name)? asc_desc?
;

create_table_stmt:
    CREATE (TEMP | TEMPORARY)? TABLE (IF NOT EXISTS)? (
        schema_name DOT
    )? table_name (
        OPEN_PAR column_def (COMMA column_def)*? (COMMA table_constraint)* CLOSE_PAR (
            WITHOUT row_ROW_ID = IDENTIFIER
        )?
        | AS select_stmt
    )
;

column_def:
    column_name type_name? column_constraint*
;

type_name:
    name+? (
        OPEN_PAR signed_number CLOSE_PAR
        | OPEN_PAR signed_number COMMA signed_number CLOSE_PAR
    )?
;

column_constraint: (CONSTRAINT name)? (
        (PRIMARY KEY asc_desc? conflict_clause? AUTOINCREMENT?)
        | (NOT? NULL | UNIQUE) conflict_clause?
        | CHECK OPEN_PAR expr CLOSE_PAR
        | DEFAULT (signed_number | literal_value | OPEN_PAR expr CLOSE_PAR)
        | COLLATE collation_name
        | foreign_key_clause
        | (GENERATED ALWAYS)? AS OPEN_PAR expr CLOSE_PAR (
            STORED
            | VIRTUAL
        )?
    )
;

signed_number: (PLUS | MINUS)? NUMERIC_LITERAL
;

table_constraint: (CONSTRAINT name)? (
        (PRIMARY KEY | UNIQUE) OPEN_PAR indexed_column (
            COMMA indexed_column
        )* CLOSE_PAR conflict_clause?
        | CHECK OPEN_PAR expr CLOSE_PAR
        | FOREIGN KEY OPEN_PAR column_name (COMMA column_name)* CLOSE_PAR foreign_key_clause
    )
;

foreign_key_clause:
    REFERENCES foreign_table (
        OPEN_PAR column_name (COMMA column_name)* CLOSE_PAR
    )? (
        ON (DELETE | UPDATE) (
            SET (NULL | DEFAULT)
            | CASCADE
            | RESTRICT
            | NO ACTION
        )
        | MATCH name
    )* (NOT? DEFERRABLE (INITIALLY (DEFERRED | IMMEDIATE))?)?
;

conflict_clause:
    ON CONFLICT (
        ROLLBACK
        | ABORT
        | FAIL
        | IGNORE
        | REPLACE
    )
;

create_trigger_stmt:
    CREATE (TEMP | TEMPORARY)? TRIGGER (IF NOT EXISTS)? (
        schema_name DOT
    )? trigger_name (BEFORE | AFTER | INSTEAD OF)? (
        DELETE
        | INSERT
        | UPDATE (OF column_name ( COMMA column_name)*)?
    ) ON table_name (FOR EACH ROW)? (WHEN expr)? BEGIN (
        (update_stmt | insert_stmt | delete_stmt | select_stmt) SCOL
    )+ END
;

create_view_stmt:
    CREATE (TEMP | TEMPORARY)? VIEW (IF NOT EXISTS)? (
        schema_name DOT
    )? view_name (OPEN_PAR column_name (COMMA column_name)* CLOSE_PAR)? AS select_stmt
;

create_virtual_table_stmt:
    CREATE VIRTUAL TABLE (IF NOT EXISTS)? (schema_name DOT)? table_name USING module_name (
        OPEN_PAR module_argument (COMMA module_argument)* CLOSE_PAR
    )?
;

with_clause:
    WITH RECURSIVE? cte_table_name AS OPEN_PAR select_stmt CLOSE_PAR (
        COMMA cte_table_name AS OPEN_PAR select_stmt CLOSE_PAR
    )*
;

cte_table_name:
    table_name (OPEN_PAR column_name ( COMMA column_name)* CLOSE_PAR)?
;

recursive_cte:
    cte_table_name AS OPEN_PAR initial_select UNION ALL? recursive_select CLOSE_PAR
;

common_table_expression:
    table_name (OPEN_PAR column_name ( COMMA column_name)* CLOSE_PAR)? AS OPEN_PAR select_stmt CLOSE_PAR
;

delete_stmt:
    with_clause? DELETE FROM qualified_table_name (WHERE expr)? returning_clause?
;

delete_stmt_limited:
    with_clause? DELETE FROM qualified_table_name (WHERE expr)? returning_clause? (
        order_by_clause? limit_clause
    )?
;

detach_stmt:
    DETACH DATABASE? schema_name
;

drop_stmt:
    DROP object = (INDEX | TABLE | TRIGGER | VIEW) (
        IF EXISTS
    )? (schema_name DOT)? any_name
;

/*
 SQLite understands the following binary operators, in order from highest to lowest precedence:
    ||
    * / %
    + -
    << >> & |
    < <= > >=
    = == != <> IS IS NOT IN LIKE GLOB MATCH REGEXP
    AND
    OR
 */
expr:
    literal_value
    | BIND_PARAMETER
    | ((schema_name DOT)? table_name DOT)? column_name
    | unary_operator expr
    | expr PIPE2 expr
    | expr ( STAR | DIV | MOD) expr
    | expr ( PLUS | MINUS) expr
    | expr ( LT2 | GT2 | AMP | PIPE) expr
    | expr ( LT | LT_EQ | GT | GT_EQ) expr
    | expr (
        ASSIGN
        | EQ
        | NOT_EQ1
        | NOT_EQ2
        | IS
        | IS NOT
        | IN
        | LIKE
        | GLOB
        | MATCH
        | REGEXP
    ) expr
    | expr AND expr
    | expr OR expr
    | function_name OPEN_PAR ((DISTINCT? expr ( COMMA expr)*) | STAR)? CLOSE_PAR filter_clause? over_clause?
    | OPEN_PAR expr (COMMA expr)* CLOSE_PAR
    | CAST OPEN_PAR expr AS type_name CLOSE_PAR
    | expr COLLATE collation_name
    | expr NOT? (LIKE | GLOB | REGEXP | MATCH) expr (
        ESCAPE expr
    )?
    | expr ( ISNULL | NOTNULL | NOT NULL)
    | expr IS NOT? expr
    | expr NOT? BETWEEN expr AND expr
    | expr NOT? IN (
        OPEN_PAR (select_stmt | expr ( COMMA expr)*)? CLOSE_PAR
        | ( schema_name DOT)? table_name
        | (schema_name DOT)? table_function_name OPEN_PAR (expr (COMMA expr)*)? CLOSE_PAR
    )
    | ((NOT)? EXISTS)? OPEN_PAR select_stmt CLOSE_PAR
    | CASE expr? (WHEN expr THEN expr)+ (ELSE expr)? END
    | raise_function
;

raise_function:
    RAISE OPEN_PAR (
        IGNORE
        | (ROLLBACK | ABORT | FAIL) COMMA error_message
    ) CLOSE_PAR
;

literal_value:
    NUMERIC_LITERAL
    | STRING_LITERAL
    | BLOB_LITERAL
    | NULL
    | TRUE
    | FALSE
    | CURRENT_TIME
    | CURRENT_DATE
    | CURRENT_TIMESTAMP
;

columns_clause:
    OPEN_PAR column_name (COMMA column_name)* CLOSE_PAR
;



insert_stmt:
    with_clause? (
        INSERT
        | REPLACE
        | INSERT OR (
            REPLACE
            | ROLLBACK
            | ABORT
            | FAIL
            | IGNORE
        )
    ) INTO (schema_name DOT)? table_name (AS table_alias)? 
        columns_clause? (
        (
            (values_clause | select_stmt) upsert_clause?
        )
        | DEFAULT VALUES
    ) returning_clause?
;

returning_clause:
    RETURNING result_column (COMMA result_column)*
;

upsert_clause:
    ON CONFLICT (
        OPEN_PAR indexed_column (COMMA indexed_column)* CLOSE_PAR (WHERE expr)?
    )? DO (
        NOTHING
        | UPDATE SET (
            (column_name | column_name_list) ASSIGN expr (
                COMMA (column_name | column_name_list) ASSIGN expr
            )* (WHERE expr)?
        )
    )
;

pragma_stmt:
    PRAGMA (schema_name DOT)? pragma_name (
        ASSIGN pragma_value
        | OPEN_PAR pragma_value CLOSE_PAR
    )?
;

pragma_value:
    signed_number
    | name
    | STRING_LITERAL
;

reindex_stmt:
    REINDEX (collation_name | (schema_name DOT)? (table_name | index_name))?
;

select_stmt:
    common_table_stmt? select_core (compound_operator select_core)* order_by_clause? limit_clause?
;

join_clause:
    (join_operator table_or_subquery join_constraint?)+
;

where_clause:
    WHERE expr
;

group_by_clause:
    GROUP BY expr (COMMA expr)* having_clause?
;

having_clause:
    HAVING expr
;

values_clause:
    VALUES values_stmt (COMMA values_stmt)*
;

values_stmt:
    OPEN_PAR expr ( COMMA expr)* CLOSE_PAR
;

from_clause:
    FROM table_or_subquery (COMMA table_or_subquery)* join_clause?
;

window_clause:
    WINDOW window_name AS window_defn (
                    COMMA window_name AS window_defn
                )* | window_defn (
                    COMMA window_defn
                )*
;

select_core:
    (
        SELECT (DISTINCT | ALL)? result_column (COMMA result_column)* 
         from_clause? where_clause? group_by_clause? window_clause?
    )
    | values_clause
;

factored_select_stmt:
    select_stmt
;

simple_select_stmt:
    common_table_stmt? select_core order_by_clause? limit_clause?
;

compound_select_stmt:
    common_table_stmt? select_core (
        (UNION ALL? | INTERSECT | EXCEPT) select_core
    )+ order_by_clause? limit_clause?
;

table_or_subquery: (
        (schema_name DOT)? table_name (AS? table_alias)? (
            INDEXED BY index_name
            | NOT INDEXED
        )?
    )
    | (schema_name DOT)? table_function_name OPEN_PAR expr (COMMA expr)* CLOSE_PAR (
        AS? table_alias
    )?
    | OPEN_PAR (table_or_subquery (COMMA table_or_subquery)* | join_clause) CLOSE_PAR
    | OPEN_PAR select_stmt CLOSE_PAR (AS? table_alias)?
;

result_column:
    STAR
    | table_name DOT STAR
    | expr ( AS? column_alias)?
;

join_operator:
    COMMA
    | NATURAL? (LEFT OUTER? | INNER | CROSS)? JOIN
;

join_constraint:
    ON expr
    | USING OPEN_PAR column_name ( COMMA column_name)* CLOSE_PAR
;

compound_operator:
    UNION ALL?
    | INTERSECT
    | EXCEPT
;

set_clause:
    SET set_stmt (COMMA set_stmt)*
;
set_stmt:
    (column_name | column_name_list) ASSIGN expr
;

update_stmt:
    with_clause? UPDATE (
        OR (ROLLBACK | ABORT | REPLACE | FAIL | IGNORE)
    )? qualified_table_name set_clause (
        FROM (table_or_subquery (COMMA table_or_subquery)* | join_clause)
    )? where_clause? returning_clause?
;

column_name_list:
    OPEN_PAR column_name (COMMA column_name)* CLOSE_PAR
;

update_stmt_limited:
    with_clause? UPDATE (
        OR (ROLLBACK | ABORT | REPLACE | FAIL | IGNORE)
    )? qualified_table_name SET (column_name | column_name_list) ASSIGN expr (
        COMMA (column_name | column_name_list) ASSIGN expr
    )* (WHERE expr)? returning_clause? (order_by_clause? limit_clause)?
;

qualified_table_name: (schema_name DOT)? table_name (AS alias)? (
        INDEXED BY index_name
        | NOT INDEXED
    )?
;

vacuum_stmt:
    VACUUM schema_name? (INTO filename)?
;

filter_clause:
    FILTER OPEN_PAR WHERE expr CLOSE_PAR
;

window_defn:
    OPEN_PAR base_window_name? (PARTITION BY expr (COMMA expr)*)? (
        ORDER BY ordering_term (COMMA ordering_term)*
    ) frame_spec? CLOSE_PAR
;

over_clause:
    OVER (
        window_name
        | OPEN_PAR base_window_name? (PARTITION BY expr (COMMA expr)*)? (
            ORDER BY ordering_term (COMMA ordering_term)*
        )? frame_spec? CLOSE_PAR
    )
;

frame_spec:
    frame_clause (
        EXCLUDE (NO OTHERS)
        | CURRENT ROW
        | GROUP
        | TIES
    )?
;

frame_clause: (RANGE | ROWS | GROUPS) (
        frame_single
        | BETWEEN frame_left AND frame_right
    )
;

simple_function_invocation:
    simple_func OPEN_PAR (expr (COMMA expr)* | STAR) CLOSE_PAR
;

aggregate_function_invocation:
    aggregate_func OPEN_PAR (DISTINCT? expr (COMMA expr)* | STAR)? CLOSE_PAR filter_clause?
;

window_function_invocation:
    window_function OPEN_PAR (expr (COMMA expr)* | STAR)? CLOSE_PAR filter_clause? OVER (
        window_defn
        | window_name
    )
;

common_table_stmt: //additional structures
    WITH RECURSIVE? common_table_expression (COMMA common_table_expression)*
;

order_by_clause:
    ORDER BY ordering_term (COMMA ordering_term)*
;

limit_clause:
    LIMIT expr offset_clause?
;

offset_clause:
    OFFSET expr
;


ordering_term:
    expr (COLLATE collation_name)? asc_desc? (NULLS (FIRST | LAST))?
;

asc_desc:
    ASC
    | DESC
;

frame_left:
    expr PRECEDING
    | expr FOLLOWING
    | CURRENT ROW
    | UNBOUNDED PRECEDING
;

frame_right:
    expr PRECEDING
    | expr FOLLOWING
    | CURRENT ROW
    | UNBOUNDED FOLLOWING
;

frame_single:
    expr PRECEDING
    | UNBOUNDED PRECEDING
    | CURRENT ROW
;

// unknown

window_function:
    (FIRST_VALUE | LAST_VALUE) OPEN_PAR expr CLOSE_PAR OVER OPEN_PAR partition_by? order_by_expr_asc_desc frame_clause
        ? CLOSE_PAR
    | (CUME_DIST | PERCENT_RANK) OPEN_PAR CLOSE_PAR OVER OPEN_PAR partition_by? order_by_expr? CLOSE_PAR
    | (DENSE_RANK | RANK | ROW_NUMBER) OPEN_PAR CLOSE_PAR OVER OPEN_PAR partition_by? order_by_expr_asc_desc
        CLOSE_PAR
    | (LAG | LEAD) OPEN_PAR expr offset? default_value? CLOSE_PAR OVER OPEN_PAR partition_by?
        order_by_expr_asc_desc CLOSE_PAR
    | NTH_VALUE OPEN_PAR expr COMMA signed_number CLOSE_PAR OVER OPEN_PAR partition_by? order_by_expr_asc_desc
        frame_clause? CLOSE_PAR
    | NTILE OPEN_PAR expr CLOSE_PAR OVER OPEN_PAR partition_by? order_by_expr_asc_desc CLOSE_PAR
;

offset:
    COMMA signed_number
;

default_value:
    COMMA signed_number
;

partition_by:
    PARTITION BY expr+
;

order_by_expr:
    ORDER BY expr+
;

order_by_expr_asc_desc:
    ORDER BY expr_asc_desc
;

expr_asc_desc:
    expr asc_desc? (COMMA expr asc_desc?)*
;

//TODO BOTH OF THESE HAVE TO BE REWORKED TO FOLLOW THE SPEC
initial_select:
    select_stmt
;

recursive_select:
    select_stmt
;

unary_operator:
    MINUS
    | PLUS
    | TILDE
    | NOT
;

error_message:
    STRING_LITERAL
;

module_argument: // TODO check what exactly is permitted here
    expr
    | column_def
;

column_alias:
    IDENTIFIER
    | STRING_LITERAL
;

keyword:
    ABORT
    | ACTION
    | ADD
    | AFTER
    | ALL
    | ALTER
    | ANALYZE
    | AND
    | AS
    | ASC
    | ATTACH
    | AUTOINCREMENT
    | BEFORE
    | BEGIN
    | BETWEEN
    | BY
    | CASCADE
    | CASE
    | CAST
    | CHECK
    | COLLATE
    | COLUMN
    | COMMIT
    | CONFLICT
    | CONSTRAINT
    | CREATE
    | CROSS
    | CURRENT_DATE
    | CURRENT_TIME
    | CURRENT_TIMESTAMP
    | DATABASE
    | DEFAULT
    | DEFERRABLE
    | DEFERRED
    | DELETE
    | DESC
    | DETACH
    | DISTINCT
    | DROP
    | EACH
    | ELSE
    | END
    | ESCAPE
    | EXCEPT
    | EXCLUSIVE
    | EXISTS
    | EXPLAIN
    | FAIL
    | FOR
    | FOREIGN
    | FROM
    | FULL
    | GLOB
    | GROUP
    | HAVING
    | IF
    | IGNORE
    | IMMEDIATE
    | IN
    | INDEX
    | INDEXED
    | INITIALLY
    | INNER
    | INSERT
    | INSTEAD
    | INTERSECT
    | INTO
    | IS
    | ISNULL
    | JOIN
    | KEY
    | LEFT
    | LIKE
    | LIMIT
    | MATCH
    | NATURAL
    | NO
    | NOT
    | NOTNULL
    | NULL
    | OF
    | OFFSET
    | ON
    | OR
    | ORDER
    | OUTER
    | PLAN
    | PRAGMA
    | PRIMARY
    | QUERY
    | RAISE
    | RECURSIVE
    | REFERENCES
    | REGEXP
    | REINDEX
    | RELEASE
    | RENAME
    | REPLACE
    | RESTRICT
    | RIGHT
    | ROLLBACK
    | ROW
    | ROWS
    | SAVEPOINT
    | SELECT
    | SET
    | TABLE
    | TEMP
    | TEMPORARY
    | THEN
    | TO
    | TRANSACTION
    | TRIGGER
    | UNION
    | UNIQUE
    | UPDATE
    | USING
    | VACUUM
    | VALUES
    | VIEW
    | VIRTUAL
    | WHEN
    | WHERE
    | WITH
    | WITHOUT
    | FIRST_VALUE
    | OVER
    | PARTITION
    | RANGE
    | PRECEDING
    | UNBOUNDED
    | CURRENT
    | FOLLOWING
    | CUME_DIST
    | DENSE_RANK
    | LAG
    | LAST_VALUE
    | LEAD
    | NTH_VALUE
    | NTILE
    | PERCENT_RANK
    | RANK
    | ROW_NUMBER
    | GENERATED
    | ALWAYS
    | STORED
    | TRUE
    | FALSE
    | WINDOW
    | NULLS
    | FIRST
    | LAST
    | FILTER
    | GROUPS
    | EXCLUDE
;

// TODO: check all names below

name:
    any_name
;

function_name:
    any_name
;

schema_name:
    any_name
;

table_name:
    any_name
;

table_or_index_name:
    any_name
;

column_name:
    any_name
;

collation_name:
    any_name
;

foreign_table:
    any_name
;

index_name:
    any_name
;

trigger_name:
    any_name
;

view_name:
    any_name
;

module_name:
    any_name
;

pragma_name:
    any_name
;

savepoint_name:
    any_name
;

table_alias:
    any_name
;

transaction_name:
    any_name
;

window_name:
    any_name
;

alias:
    any_name
;

filename:
    any_name
;

base_window_name:
    any_name
;

simple_func:
    any_name
;

aggregate_func:
    any_name
;

table_function_name:
    any_name
;

any_name:
    IDENTIFIER
    | keyword
    | STRING_LITERAL
    | OPEN_PAR any_name CLOSE_PAR
;