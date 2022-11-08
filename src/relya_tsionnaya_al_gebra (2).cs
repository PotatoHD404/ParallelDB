// реля ционная ал гебра

// что хотимб??

var domFloat = Domain
    .FromClrType("Float", typeof(float))

var domAge = Domain
    .Define("Age")
    .Parent(domFloat)
    .Constraint(x => x >= 0.0);

var rel1 = Relation
    .Define("Relation1")
    .Attribute("Attr1", domFloat)
        .Constraint(x => x >= 10.0)
        .Nullable(false)
        .Default(25.0)
        .Default(x => r["Attr2"] + r["Attr3"])
        .End()
    .Attribute("Attr2", domFloat)
    .Attribute("Attr3", domFloat)
    .Attribute("Attr4", domFloat)
    .Attribute("Attr5", domFloat)
    .PotentialKey("Attr1", "Attr2", "Attr3")
    .PotentialKey("Attr1", "Attr2", "Attr4");

rel1.PotentialKey(rel1["Attr1"], rel1["Attr2"]);

// Альтернативный вариант
// var rel1 = Relation 
//     .Define()
//     .Name("Relation1")

// мб юзнуть рефлексию

var rel2 = Relation
    .Define()
    // ...
    // ...
    // ...
    .End();

rel1.ForeignKey("...", new Tuple(rel1["Attr1"]), new  Tuple(rel2["Attr2"])); 

// rel1 type : RelationExpression, так как не вызван End()

// Tuple для составных ключей нужны, для обычных можно юзать просто значения 

// можно использовать массивы вместо Tuple

var rrel = rel1.End(); // type: Relation

// Деление - в топку 

// Деление требует два вспомогательных бинарных отношения? поискать у кристофера дейта

var query = rel1
    .Where(r => p(r))
    .Union(r3)
    .Join(rel2, (r1, r2) => p(r1, r2))
    .Project(a1, a2, a3); // QueryExpression
    .EquiJoin()
        .Left()
        .Right()

    // для гарантии что методы вызовутся в правильном порядке - возвращать особые QueryExpression

// query.Execute() // не стоит так делать из за сложности реализации для многопоточности

var engine = new /*(Sync/Async)*/RelationalEngine();

var rel222 = engine.Execute(query); // Query result;

// Что за RelationExpression, как выглядит метод Define

public static class Relation {
    public static RelationExpression Define(string relationName) {
        return new RelationExpression(relationName);
    }
}

public class RelationExpression {
    private string _relationName;
    internal RelationExpression(string relationName) {
        _relationName = relationName;
    }

    public AttributeExpression Attribute(string attributeName, Domain domain) {
        return new AttributeExpression(this, attributeName, domain);
    }

    public AttributeExpression Attribute(string attributeName, DomainExpression domain) {
        return new AttributeExpression(t//his, attributeName, domain);
    }

    public RelationExpression PotentialKey(params AttributeExpression[] attributes) {
        // ...
        return this;
    }
    // мб добавить name
    public RelationExpression ForeignKey(ICollection<RelationAttribute> from, ICollection<RelationAttribute>  to) {
        // ...
        return this;
    }

    public Relation End() {
        // ...
        // возникает сложность при рекурсивных отношениях
        // решается добавлением внешней конструкции, которая в какой то момент вызывает End, создав объекты

        // return new Relation(_relationName, _attributes, _keys, _foreignKeys);
    }
}

// Это DSL для описания отношений

// Сначала он тестируется, потом только создается язык, синтаксический анализатор

public class AttributeExpression {
    private RelationExpression _relation;
    private string _attributeName;
    private Domain _domain;

    internal AttributeExpression(RelationExpression relation, string attributeName, Domain domain) {
        _relation = relation;
        _attributeName = attributeName;
        _domain = domain;
    }

    public AttributeExpression Constraint(Func<object, bool> constraint) {
        // ...
        return this;
    }

    public AttributeExpression Nullable(bool nullable) {
        // ...
        return this;
    }

    public AttributeExpression Default(object defaultValue) {
        // ...
        return this;
    }

    public AttributeExpression Default(Func<Relation, object> defaultValue) {
        // ...

        return this;
    }

    public RelationExpression End() {
        return _relation;
    }
}

// convert code above to golang

// define struct for RelationExpression

struct RelationExpression {
    relationName string
}

func (r RelationExpression) Attribute(attributeName string, domain Domain) AttributeExpression {
    return AttributeExpression{r, attributeName, domain}
}

func (r RelationExpression) PotentialKey(attributes ...AttributeExpression) RelationExpression {
    // ...
    return r
}

func (r RelationExpression) ForeignKey(from, to []RelationAttribute) RelationExpression {
    // ...
    return r
}

func (r RelationExpression) End() Relation {
    // ...
    // возникает сложность при рекурсивных отношениях
    // решается добавлением внешней конструкции, которая в какой то момент вызывает End, создав объекты

    // return new Relation(_relationName, _attributes, _keys, _foreignKeys);
}

// define struct for AttributeExpression

struct AttributeExpression {
    relation RelationExpression
    attributeName string
    domain Domain
}

func (a AttributeExpression) Constraint(constraint func(object) bool) AttributeExpression {
    // ...
    return a
}

func (a AttributeExpression) Nullable(nullable bool) AttributeExpression {
    // ...
    return a
}

func (a AttributeExpression) Default(defaultValue interface{}) AttributeExpression {
    // ...
    return a
}

func (a AttributeExpression) Default(defaultValue func(Relation) interface{}) AttributeExpression {
    // ...
    return a
}

func (a AttributeExpression) End() RelationExpression {
    return a.relation
}

// define struct for Relation

struct Relation {
    relationName string
    attributes []Attribute
    keys []Attribute
    foreignKeys []ForeignKey
}

// define struct for Attribute

struct Attribute {
    attributeName string
    domain Domain
    constraint func(object) bool
    nullable bool
    defaultValue interface{}
}

// define struct for ForeignKey

struct ForeignKey {
    from []RelationAttribute
    to []RelationAttribute
}

// define struct for RelationAttribute

struct RelationAttribute {
    relation Relation
    attribute Attribute
}

// define interface for Domain

type Domain interface {
    // ...
}

// define struct for DomainExpression

// add realisation for Domain interface

func (d DomainExpression) End() Domain {
    // ...
}
