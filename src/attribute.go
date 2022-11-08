package parallelDB

type Attribute struct {
	attributeName string
	domain        Domain
	nullable      bool
	defaultValue  interface{}
}
