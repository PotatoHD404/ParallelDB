package parallelDB

type Relation struct {
	relationName string
	attributes   []Attribute
	keys         []Attribute
	foreignKeys  []ForeignKey
}
