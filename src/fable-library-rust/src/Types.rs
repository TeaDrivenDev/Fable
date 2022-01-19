// This file is for type alias declarations
// Please keep in sync with generated types
// TODO: remove when type alias entities are supported
#![allow(non_snake_case)]
use std::rc::Rc;

pub type List_1<T> = Option<Rc<crate::List::Node_1<T>>>;
pub type Set_1<T> = Option<Rc<crate::Set::SetTree_1<T>>>;
pub type Map_2<K, V> = Option<Rc<crate::Map::MapTree_2<K, V>>>;

pub use crate::Choice::*;